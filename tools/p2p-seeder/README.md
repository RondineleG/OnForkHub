# OnForkHub P2P seeder (experimental)

This worker is an experimental WebRTC-capable seeding prototype for authorized public MP4/WebM videos. It uses webtorrent-hybrid 6.0.0, which is deprecated but documents WebRTC support in Node.js. Actual interoperability has not yet been verified.

## Local test

```bash
cd tools/p2p-seeder
mkdir -p media metadata
cp /path/to/authorized-test.mp4 media/test.mp4
docker compose up --build
```

The worker scans `/media`, starts seeding, and writes `filename.mp4.torrent` and `filename.mp4.magnet` to `/metadata`. Copy the generated magnet into a test video record to verify the existing Blazor player.

**Important limitations:** the upload storage service is not connected to this read-only seeder directory yet. An operator must place a file there for now. The output files do not mean browsers can connect: validate at least one WebRTC peer on a separate network, matching WSS trackers, tracker availability, container networking/NAT, and codec/browser compatibility. Peer changes are not continuously exported as metrics.

Use a persistent media directory and never mount arbitrary user input as host paths. For staging, make sure the seeder's container can reach public trackers. The worker checks `WebTorrent.WEBRTC_SUPPORT` at startup and aborts if the runtime cannot serve WebRTC peers. This is only a startup gate, not proof of browser-to-seeder transport. Verify a real connection and video playback before deploying.

Before production: add explicit media lifecycle management, upload-to-seeder handoff, retry/backoff and failure marking, container healthchecks, metrics, retention, and network configuration. Do not use plain torrent distribution to claim file-level privacy or revocation.
