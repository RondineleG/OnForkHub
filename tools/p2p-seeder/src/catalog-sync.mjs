import { readdir, readFile } from 'node:fs/promises';
import path from 'node:path';

const metadataDir = process.env.METADATA_DIR || '/metadata';
const baseUrl = (process.env.CATALOG_API_URL || '').replace(/\/$/, '');
const bearer = process.env.CATALOG_API_TOKEN || '';
const apply = process.env.CATALOG_SYNC_APPLY === 'true';
const pageSize = 100;

function storageFileName(video) {
  if (!video?.url || typeof video.url !== 'string') return null;
  try {
    const url = new URL(video.url, 'http://localhost');
    return decodeURIComponent(url.pathname.split('/').pop() || '');
  } catch {
    return null;
  }
}

export async function synchronizeCatalog({ directory, listVideos, enableTorrent, dryRun = true }) {
  const entries = (await readdir(directory)).filter(name => name.endsWith('.json'));
  const videos = await listVideos();
  const byName = new Map();
  for (const video of videos) {
    const name = storageFileName(video);
    if (!name) continue;
    if (!byName.has(name)) byName.set(name, []);
    byName.get(name).push(video);
  }
  const report = [];
  for (const entry of entries) {
    const data = JSON.parse(await readFile(path.join(directory, entry), 'utf8'));
    const name = data.fileName;
    if (data.schemaVersion !== 1 || typeof name !== 'string' || entry !== name + '.json' ||
        !/^magnet:\?xt=urn:btih:/i.test(data.magnetUri || '') ||
        !/^[a-f\d]{40}$/i.test(data.infoHash || '')) {
      report.push({ entry, status: 'invalid' });
      continue;
    }
    const matches = byName.get(name) || [];
    if (matches.length !== 1) {
      report.push({ entry, status: matches.length ? 'ambiguous' : 'not-found' });
      continue;
    }
    const video = matches[0];
    if (video.magnetUri === data.magnetUri && video.isTorrentEnabled) {
      report.push({ entry, status: 'unchanged' });
      continue;
    }
    if (video.isTorrentEnabled && video.magnetUri && video.magnetUri !== data.magnetUri) {
      report.push({ entry, status: 'conflict' });
      continue;
    }
    if (!dryRun) await enableTorrent(video.id, data.magnetUri);
    report.push({ entry, status: dryRun ? 'would-update' : 'updated', videoId: video.id });
  }
  return report;
}

async function listVideos() {
  const all = [];
  for (let page = 1; page <= 100; page++) {
    const response = await fetch(`${baseUrl}/api/v1/videos?page=${page}&pageSize=${pageSize}`, {
      headers: { Authorization: `Bearer ${bearer}` }
    });
    if (response.status === 204) break;
    if (!response.ok) throw new Error(`Catalog listing failed: HTTP ${response.status}`);
    const data = await response.json();
    if (!Array.isArray(data)) throw new Error('Unexpected catalog response shape');
    all.push(...data);
    if (data.length < pageSize) break;
    if (page === 100) throw new Error('Catalog pagination safety limit exceeded');
  }
  return all;
}

async function enableTorrent(id, magnetUri) {
  const response = await fetch(`${baseUrl}/api/v1/videos/${encodeURIComponent(id)}/torrent`, {
    method: 'POST',
    headers: { Authorization: `Bearer ${bearer}`, 'Content-Type': 'application/json' },
    body: JSON.stringify({ magnetUri })
  });
  if (!response.ok) throw new Error(`Cannot enable torrent for ${id}: HTTP ${response.status}`);
}

if (process.argv[1] && path.resolve(process.argv[1]) === path.resolve(new URL(import.meta.url).pathname)) {
  if (!baseUrl || !bearer) throw new Error('CATALOG_API_URL and CATALOG_API_TOKEN are required');
  synchronizeCatalog({ directory: metadataDir, listVideos, enableTorrent, dryRun: !apply })
    .then(results => console.log(JSON.stringify({ dryRun: !apply, results }, null, 2)))
    .catch(error => { console.error(error); process.exitCode = 1; });
}
