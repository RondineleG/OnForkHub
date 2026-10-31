import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../src/Presentations/OnForkHub.Web/wwwroot/js/webtorrentService.js', import.meta.url), 'utf8');
const { startDownload, stopDownload, getTorrentStats } = await import('data:text/javascript;base64,' + Buffer.from(source).toString('base64'));

// Minimal browser stand-ins: a <video> that fires media events, a container and an already-active service worker.
class FakeVideo extends EventTarget {
  constructor() { super(); this.error = null; }
}

const containers = new Map([['video', { children: [], appendChild(child) { this.children.push(child); } }]]);
globalThis.document = {
  getElementById: (id) => containers.get(id) ?? null,
  createElement: () => new FakeVideo()
};

let workerRegistrations = 0;
Object.defineProperty(globalThis, 'navigator', {
  configurable: true,
  value: {
    serviceWorker: {
      controller: {},
      async register(url, options) {
        workerRegistrations++;
        assert.equal(url, '/webtorrent-sw.min.js');
        assert.deepEqual(options, { scope: '/' });
        return { active: { state: 'activated' } };
      },
      addEventListener() {}
    }
  }
});

// streamTo() makes the fake video playable on the next tick, like the browser firing 'canplay'.
const playableFile = (name = 'sample.mp4', length = 10) => ({
  name,
  length,
  streamTo(video) { video.src = `/webtorrent/hash/${name}`; queueMicrotask(() => video.dispatchEvent(new Event('canplay'))); }
});

const torrents = new Map();
let serverController = null;
const client = {
  createServer({ controller }) { serverController = controller; },
  add(uri, options, callback) {
    assert.equal(typeof options.store, 'function', 'pieces are kept in the in-memory store');
    const torrent = {
      files: [playableFile()],
      numPeers: 3, progress: 0.5, downloadSpeed: 1024, uploadSpeed: 0,
      on() {},
      removeListener() {}
    };
    torrents.set(uri, torrent);
    queueMicrotask(() => callback(torrent));
    return torrent;
  },
  remove(uri, cb) { torrents.delete(uri); cb?.(); }
};
globalThis.WebTorrent = class { constructor() { return client; } };

const withAdd = async (add, run) => {
  const original = client.add;
  client.add = add;
  try { await run(); } finally { client.add = original; }
};

test('rejects when video container is absent', async () => {
  await assert.rejects(startDownload('missing', 'bad-id'), /container not found/);
});

test('registers the stream worker once and serves through it', async () => {
  await startDownload('first', 'video');
  await startDownload('second', 'video');
  assert.equal(workerRegistrations, 1);
  assert.deepEqual(serverController, { active: { state: 'activated' } });
  stopDownload('first');
  stopDownload('second');
});

test('resolves with a streaming video once it can play', async () => {
  const video = await startDownload('success', 'video');
  assert.ok(video instanceof FakeVideo);
  assert.equal(video.src, '/webtorrent/hash/sample.mp4');
  assert.ok(containers.get('video').children.includes(video));
  stopDownload('success');
  assert.equal(torrents.has('success'), false);
});

test('applies playback options to the created video', async () => {
  const video = await startDownload('options', 'video', { autoplay: false, controls: false, muted: true, preload: 'metadata' });
  assert.deepEqual(
    { autoplay: video.autoplay, controls: video.controls, muted: video.muted, preload: video.preload, playsInline: video.playsInline },
    { autoplay: false, controls: false, muted: true, preload: 'metadata', playsInline: true }
  );
  stopDownload('options');
});

test('streams the largest playable file', async () => {
  await withAdd((uri, options, callback) => {
    const torrent = { files: [playableFile('sample.mp4', 5), { name: 'notes.txt', length: 50 }, playableFile('movie.webm', 20)], on() {}, removeListener() {} };
    torrents.set(uri, torrent);
    queueMicrotask(() => callback(torrent));
    return torrent;
  }, async () => {
    const video = await startDownload('largest', 'video');
    assert.equal(video.src, '/webtorrent/hash/movie.webm');
    stopDownload('largest');
  });
});

test('rejects unsupported video format', async () => {
  await withAdd((uri, options, callback) => {
    const torrent = { files: [{ name: 'notes.txt' }], on() {}, removeListener() {} };
    torrents.set(uri, torrent);
    queueMicrotask(() => callback(torrent));
    return torrent;
  }, async () => {
    await assert.rejects(startDownload('unsupported', 'video'), /no playable/);
  });
});

test('rejects when the browser cannot play the video', async () => {
  await withAdd((uri, options, callback) => {
    const file = {
      name: 'broken.mp4',
      streamTo(video) { video.error = { message: 'MEDIA_ERR_SRC_NOT_SUPPORTED' }; queueMicrotask(() => video.dispatchEvent(new Event('error'))); }
    };
    const torrent = { files: [file], on() {}, removeListener() {}, destroy() { torrents.delete(uri); } };
    torrents.set(uri, torrent);
    queueMicrotask(() => callback(torrent));
    return torrent;
  }, async () => {
    await assert.rejects(startDownload('broken', 'video'), /MEDIA_ERR_SRC_NOT_SUPPORTED/);
    assert.equal(torrents.has('broken'), false);
  });
});

test('stopDownload rejects in-progress initialization', async () => {
  await withAdd((uri) => {
    const torrent = { on() {}, removeListener() {} };
    torrents.set(uri, torrent);
    return torrent;
  }, async () => {
    const pending = startDownload('cancelled', 'video');
    await new Promise((resolve) => setImmediate(resolve));
    stopDownload('cancelled');
    await assert.rejects(pending, /cancelled/);
    assert.equal(torrents.has('cancelled'), false);
  });
});

test('rejects duplicate sessions for the same magnet', async () => {
  await withAdd((uri) => {
    const torrent = { on() {}, removeListener() {} };
    torrents.set(uri, torrent);
    return torrent;
  }, async () => {
    const first = startDownload('duplicate', 'video');
    await assert.rejects(startDownload('duplicate', 'video'), /already loading/);
    stopDownload('duplicate');
    await assert.rejects(first, /cancelled/);
  });
});

test('reports torrent errors and releases the session', async () => {
  let handler;
  await withAdd((uri) => {
    const torrent = { on(event, fn) { if (event === 'error') handler = fn; }, removeListener() {} };
    torrents.set(uri, torrent);
    return torrent;
  }, async () => {
    const pending = startDownload('error-case', 'video');
    await new Promise((resolve) => setImmediate(resolve));
    handler(new Error('tracker failed'));
    await assert.rejects(pending, /tracker failed/);
    assert.equal(torrents.has('error-case'), false);
  });
});

test('reports stats for active sessions started from a .torrent URL', async () => {
  const url = 'https://example.com/sample.torrent';
  await startDownload(url, 'video');
  assert.deepEqual(getTorrentStats(url), { ready: false, peers: 3, progress: 50, downloadSpeed: 1024, uploadSpeed: 0 });
  stopDownload(url);
  assert.equal(getTorrentStats(url), null);
});

test('stopDownload destroys the torrent object when available', async () => {
  let destroyed = false;
  await withAdd(() => ({ on() {}, removeListener() {}, destroy() { destroyed = true; } }), async () => {
    const pending = startDownload('destroyable', 'video');
    await new Promise((resolve) => setImmediate(resolve));
    stopDownload('destroyable');
    await assert.rejects(pending, /cancelled/);
    assert.equal(destroyed, true);
  });
});
