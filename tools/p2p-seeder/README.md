# OnForkHub P2P seeder (experimental)

This worker is an experimental WebRTC-capable seeding prototype for authorized public MP4/WebM videos. It uses webtorrent-hybrid 6.0.0, which is deprecated but documents WebRTC support in Node.js. Actual interoperability has not yet been verified.

## Local test

```bash
cd tools/p2p-seeder
mkdir -p metadata
# Must match the API's LocalFileStorageService path: <API working directory>/<FileStorageOptions.BasePath>/videos
export ONFORKHUB_VIDEO_DIR=/absolute/path/to/api/storage/videos
docker compose up --build
```

The worker mounts the API's existing local video storage directory **read-only**. The path is required and the compose file refuses to start if it is missing. The worker scans `/media`, starts seeding, and writes `filename.mp4.torrent` and `filename.mp4.magnet` to `/metadata`. After confirming matching file metadata, copy the generated magnet into a test video record to verify the existing Blazor player.

**Important limitations:** this integration works only with the local file storage provider, when the API and seeder can access the same mounted directory. Azure Blob Storage needs a separate downloader/synchronizer; do not mount an empty directory and assume uploads arrive there. The output files do not mean browsers can connect: validate at least one WebRTC peer on a separate network, matching WSS trackers, tracker availability, container networking/NAT, and codec/browser compatibility. Peer changes are not continuously exported as metrics.

Use a persistent media directory and never mount arbitrary user input as host paths. For staging, make sure the seeder's container can reach public trackers. The worker checks `WebTorrent.WEBRTC_SUPPORT` at startup and aborts if the runtime cannot serve WebRTC peers. This is only a startup gate, not proof of browser-to-seeder transport. Verify a real connection and video playback before deploying.

Before production: add explicit media lifecycle management, upload-to-seeder handoff, retry/backoff and failure marking, container healthchecks, metrics, retention, and network configuration. Do not use plain torrent distribution to claim file-level privacy or revocation.

## Metadata handoff contract

For `<stored-file>.mp4`, the seeder writes `<stored-file>.mp4.json` after the torrent and magnet artifacts are generated:

```json
{
  "schemaVersion": 1,
  "fileName": "stored-file.mp4",
  "infoHash": "<computed-info-hash>",
  "magnetUri": "<computed-magnet>",
  "generatedAt": "<UTC-ISO8601>"
}
```

The JSON is written to a temporary file then renamed so readers do not ingest an incomplete JSON document. When a video is removed from the shared media directory, the associated metadata artifacts are removed on the next scan.

**Catalog integration is pending:** a trusted backend synchronizer must match `fileName` with the storage-generated file ID, resolve the correct video record and invoke the existing `IVideoService.EnableTorrentAsync` after verifying that the file and torrent belong together. Do not post these records directly to the public API or assume the storage URL can be replaced with a magnet without preserving a distinct HTTP fallback reference.

## Catalog synchronization (opt-in)

Run from the `tools/p2p-seeder` directory with Node.js 22, after manifest files exist:

```bash
export METADATA_DIR=/absolute/path/to/metadata
export CATALOG_API_URL=https://your-api.example
export CATALOG_API_TOKEN=your-authorized-bearer-token
npm run sync-catalog
```

Default execution is a **dry run** and does not change the catalog. It checks one-to-one correspondence between the storage-generated filename (from the video's HTTP URL) and `manifest.fileName`. Ambiguous, missing, invalid or conflicting entries are reported but not changed.

Only after inspecting the dry-run output and verifying that the token is authorized to manage exactly those videos should you run:

```bash
CATALOG_SYNC_APPLY=true npm run sync-catalog
```

This is an operator-controlled prototype, not an automated background task. The current catalog API's torrent enable endpoint must receive ownership/administrative authorization checks before unattended operation. Do not store API tokens in Compose files or commit them to Git. The HTTP URL remains unchanged; enabling torrent populates the separate MagnetUri property. Test all behavior in a non-production environment.
