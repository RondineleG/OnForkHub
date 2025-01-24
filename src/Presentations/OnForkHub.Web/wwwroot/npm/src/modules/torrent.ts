declare const WebTorrent: any;
let client: any = null;
let currentTorrent: any = null;

export async function initTorrentPlayer(progressElement: HTMLElement): Promise<void> {
    try {
        client = new WebTorrent();
        progressElement.textContent = "Ready to play";
    } catch (error) {
        throw error;
    }
}

export async function startDownload(
    progressElement: HTMLElement,
    videoContainerSelector: string,
    magnetUri: string
): Promise<void> {
    try {
        if (!client) {
            client = new WebTorrent();
        }

        if (currentTorrent) {
            currentTorrent.destroy();
            currentTorrent = null;
        }

        const container = document.querySelector(videoContainerSelector) as HTMLElement;
        container.innerHTML = `
            <video controls style="width:100%; height:100%; background-color: #000;">
                Your browser does not support video playback.
            </video>
        `;

        const videoElement = container.querySelector('video') as HTMLVideoElement;

        client.add(magnetUri, (torrent: any) => {
            const file = torrent.files.find((f: any) => {
                return /\.(mp4|mkv|webm)$/i.test(f.name);
            });

            if (!file) {
                throw new Error('No video file found');
            }

            const stream = file.createReadStream();
            const mediaSource = new MediaSource();
            videoElement.src = URL.createObjectURL(mediaSource);

            mediaSource.addEventListener('sourceopen', () => {
                const sourceBuffer = mediaSource.addSourceBuffer('video/mp4; codecs="avc1.42E01E, mp4a.40.2"');
                stream.on('data', (chunk: Uint8Array) => {
                    if (!sourceBuffer.updating) {
                        try {
                            sourceBuffer.appendBuffer(chunk);
                        } catch (e) {
                            console.warn('Error appending buffer:', e);
                        }
                    }
                });
            });

            videoElement.play().catch(console.error);

            torrent.on('download', () => {
                const progress = Math.floor(torrent.progress * 100);
                progressElement.textContent = `Downloading: ${progress}%`;

                if (videoElement.paused) {
                    videoElement.play().catch(() => { });
                }
            });

            torrent.on('done', () => {
                progressElement.textContent = 'Download complete';
            });

            currentTorrent = torrent;
        });

    } catch (error) {
        console.error('Error in startDownload:', error);
        throw error;
    }
}

export async function stopDownload(): Promise<void> {
    if (currentTorrent) {
        currentTorrent.destroy();
        currentTorrent = null;
    }

    if (client) {
        await new Promise<void>((resolve) => {
            client.destroy(() => {
                client = new WebTorrent();
                resolve();
            });
        });
    }
}