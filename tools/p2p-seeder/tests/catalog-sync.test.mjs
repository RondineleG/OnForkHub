import { test } from 'node:test';
import assert from 'node:assert/strict';
import { mkdtemp, writeFile, rm } from 'node:fs/promises';
import { tmpdir } from 'node:os';
import path from 'node:path';
import { synchronizeCatalog } from '../src/catalog-sync.mjs';

const hash = '0123456789abcdef0123456789abcdef01234567';
const magnet = 'magnet:?xt=urn:btih:' + hash;
const fixture = (fileName = 'stored.mp4') => ({ schemaVersion: 1, fileName, infoHash: hash, magnetUri: magnet });

async function withManifest(data, callback) {
  const directory = await mkdtemp(path.join(tmpdir(), 'onforkhub-sync-'));
  try {
    await writeFile(path.join(directory, data.fileName + '.json'), JSON.stringify(data));
    return await callback(directory);
  } finally {
    await rm(directory, { recursive: true, force: true });
  }
}

test('dry-run identifies a unique video without updating the catalog', async () => {
  await withManifest(fixture(), async directory => {
    let count = 0;
    const result = await synchronizeCatalog({
      directory,
      listVideos: async () => [{ id: 'video-1', url: '/files/videos/stored.mp4', isTorrentEnabled: false }],
      enableTorrent: async () => count++
    });
    assert.equal(result[0].status, 'would-update');
    assert.equal(count, 0);
  });
});

test('apply calls catalog API for a unique matching video', async () => {
  await withManifest(fixture(), async directory => {
    const calls = [];
    const result = await synchronizeCatalog({
      directory, dryRun: false,
      listVideos: async () => [{ id: 'video-1', url: '/files/videos/stored.mp4' }],
      enableTorrent: async (...args) => calls.push(args)
    });
    assert.equal(result[0].status, 'updated');
    assert.deepEqual(calls, [['video-1', magnet]]);
  });
});

test('ambiguous filename never updates catalog', async () => {
  await withManifest(fixture(), async directory => {
    const result = await synchronizeCatalog({
      directory, dryRun: false,
      listVideos: async () => [1, 2].map(id => ({ id, url: '/videos/stored.mp4' })),
      enableTorrent: async () => { throw new Error('unexpected update'); }
    });
    assert.equal(result[0].status, 'ambiguous');
  });
});

test('conflicting magnet is never overwritten', async () => {
  await withManifest(fixture(), async directory => {
    const result = await synchronizeCatalog({
      directory, dryRun: false,
      listVideos: async () => [{ id: 'a', url: '/videos/stored.mp4', isTorrentEnabled: true, magnetUri: 'other' }],
      enableTorrent: async () => { throw new Error('unexpected update'); }
    });
    assert.equal(result[0].status, 'conflict');
  });
});

test('manifest validation rejects invalid torrent hash', async () => {
  await withManifest({ ...fixture(), infoHash: 'invalid' }, async directory => {
    const result = await synchronizeCatalog({
      directory, listVideos: async () => [], enableTorrent: async () => {}
    });
    assert.equal(result[0].status, 'invalid');
  });
});
