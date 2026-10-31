import { readdir, mkdir, writeFile, stat, rename, unlink } from 'node:fs/promises';
import path from 'node:path';
import WebTorrent from 'webtorrent-hybrid';

const mediaDir = process.env.MEDIA_DIR || '/media';
const metadataDir = process.env.METADATA_DIR || '/metadata';
const pollMs = Math.max(5000, Number(process.env.POLL_MS || 30000));
const trackers = (process.env.WEBRTC_TRACKERS || 'wss://tracker.webtorrent.dev,wss://tracker.openwebtorrent.com')
  .split(',').map(x => x.trim()).filter(Boolean);
if (!WebTorrent.WEBRTC_SUPPORT) {
  throw new Error('WebRTC unavailable in the Node runtime. A WebRTC-capable seeder is required for browser playback.');
}
const client = new WebTorrent({ dht: false, lsd: false });
const active = new Map();

async function publishMetadata(name, torrent) {
  const output = {
    schemaVersion: 1,
    fileName: name,
    infoHash: torrent.infoHash,
    magnetUri: torrent.magnetURI,
    generatedAt: new Date().toISOString()
  };
  const destination = path.join(metadataDir, name + '.json');
  const temporary = destination + '.tmp';
  await writeFile(path.join(metadataDir, name + '.torrent'), torrent.torrentFile);
  await writeFile(path.join(metadataDir, name + '.magnet'), torrent.magnetURI + '\n');
  await writeFile(temporary, JSON.stringify(output, null, 2) + '\n');
  await rename(temporary, destination);
}

let closed = false;

async function synchronize() {
  const entries = await readdir(mediaDir, { withFileTypes: true });
  const files = entries.filter(x => x.isFile() && /\.(mp4|webm)$/i.test(x.name));
  const present = new Set(files.map(x => x.name));
  for (const [name, torrent] of active) {
    if (!present.has(name)) {
      if (torrent?.infoHash) client.remove(torrent.infoHash);
      active.delete(name);
      await Promise.allSettled(['.json', '.magnet', '.torrent'].map(ext => unlink(path.join(metadataDir, name + ext))));
      console.log(JSON.stringify({ event: 'unseed', name }));
    }
  }
  for (const file of files) {
    if (closed || active.has(file.name)) continue;
    const fullPath = path.join(mediaDir, file.name);
    const info = await stat(fullPath);
    if (!info.size) continue;
    // Reserve the slot before seeding, avoiding duplicate registration on slow I/O.
    active.set(file.name, null);
    try {
      client.seed(fullPath, { announce: trackers }, async torrent => {
        if (closed || !active.has(file.name)) {
          client.remove(torrent.infoHash);
          return;
        }
        active.set(file.name, torrent);
        try {
          await publishMetadata(file.name, torrent);
          console.log(JSON.stringify({ event: 'seeding', name: file.name, infoHash: torrent.infoHash, peers: torrent.numPeers }));
        } catch (error) {
          console.error('Metadata persistence failed:', error);
        }
      });
    } catch (error) {
      active.delete(file.name);
      console.error('Unable to seed:', file.name, error);
    }
  }
}

await mkdir(mediaDir, { recursive: true });
await mkdir(metadataDir, { recursive: true });
client.on('error', error => console.error('WebTorrent error:', error));
await synchronize();
const interval = setInterval(() => { synchronize().catch(error => console.error('Sync failure:', error)); }, pollMs);
const shutdown = () => {
  if (closed) return;
  closed = true;
  clearInterval(interval);
  client.destroy(() => process.exit(0));
};
process.on('SIGTERM', shutdown);
process.on('SIGINT', shutdown);
