// src/modules/torrent.ts
import * as WebTorrent from 'webtorrent';

const client = new WebTorrent();

export class TorrentHandler {
    constructor(
        private progressElement: HTMLElement,
        private videoElement: HTMLVideoElement,
        private magnetUri: string
    ) { }

    async startDownload(): Promise<void> {
        return new Promise((resolve, reject) => {
            client.add(this.magnetUri, (torrentInstance: WebTorrent.Torrent) => {
                const videoFile = torrentInstance.files.find((file: WebTorrent.TorrentFile) =>
                    file.name.endsWith('.mp4') ||
                    file.name.endsWith('.webm') ||
                    file.name.endsWith('.mkv')
                );

                if (!videoFile) {
                    reject(new Error('No video file found'));
                    return;
                }

                videoFile.renderTo(this.videoElement);

                torrentInstance.on('done', () => {
                    this.progressElement.innerHTML = 'Download complete';
                    resolve();
                });

                const interval = setInterval(() => {
                    const progress = (torrentInstance.progress * 100).toFixed(1);
                    this.progressElement.innerHTML = `Progress: ${progress}%`;
                    if (torrentInstance.progress === 1) clearInterval(interval);
                }, 1000);
            });
        });
    }

    stop(): void {
        this.videoElement.pause();
        this.videoElement.src = '';
        this.progressElement.innerHTML = 'Stopped';
        client.destroy();
    }
}

let currentHandler: TorrentHandler | null = null;

export function initTorrentPlayer(progressElement: HTMLElement, videoElement: HTMLVideoElement, magnetUri: string): void {
    currentHandler = new TorrentHandler(progressElement, videoElement, magnetUri);
}

export function startDownload(): Promise<void> {
    if (!currentHandler) throw new Error('Player not initialized');
    return currentHandler.startDownload();
}

export function stopDownload(): void {
    if (currentHandler) currentHandler.stop();
}