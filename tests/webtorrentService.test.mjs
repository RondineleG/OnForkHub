import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const source = await readFile(new URL('../src/Presentations/OnForkHub.Web/wwwroot/js/webtorrentService.js', import.meta.url), 'utf8');
const { startDownload, stopDownload } = await import('data:text/javascript;base64,' + Buffer.from(source).toString('base64'));

const listeners = new Map();
const torrents = new Map();
const client = {
  add(uri, callback) {
    const torrent = {
      files: [{ name: 'sample.mp4', appendTo(_container, done) { done(null); } }],
      on(event, fn) { listeners.set(event, fn); },
      removeListener(event) { listeners.delete(event); }
    };
    torrents.set(uri, torrent);
    queueMicrotask(() => callback(torrent));
    return torrent;
  },
  remove(uri, cb) { torrents.delete(uri); cb?.(); }
};
globalThis.WebTorrent = class { constructor() { return client; } };
globalThis.document = { getElementById: (id) => id === 'video' ? {} : null };

test('rejects when video container is absent', async () => {
  await assert.rejects(startDownload('missing', 'bad-id'), /container not found/);
});

test('resolves when a playable video is appended', async () => {
  await startDownload('success', 'video');
  assert.ok(torrents.has('success'));
  stopDownload('success');
  assert.equal(torrents.has('success'), false);
});

test('rejects unsupported video format', async () => {
  const original = client.add;
  client.add = (uri, callback) => {
    const torrent = {
      files: [{ name: 'notes.txt' }],
      on() {},
      removeListener() {}
    };
    torrents.set(uri, torrent);
    queueMicrotask(() => callback(torrent));
    return torrent;
  };
  try {
    await assert.rejects(startDownload('unsupported', 'video'), /no playable/);
  } finally { client.add = original; }
});

test('stopDownload rejects in-progress initialization', async () => {
  const original = client.add;
  client.add = (uri) => {
    const torrent = { on() {}, removeListener() {} };
    torrents.set(uri, torrent);
    return torrent;
  };
  try {
    const pending = startDownload('cancelled', 'video');
    stopDownload('cancelled');
    await assert.rejects(pending, /cancelled/);
    assert.equal(torrents.has('cancelled'), false);
  } finally { client.add = original; }
});
