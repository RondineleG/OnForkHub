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
        }

        const container = document.querySelector(videoContainerSelector) as HTMLElement;
        container.innerHTML = '';

        const videoElement = document.createElement('video');
        videoElement.controls = true;
        videoElement.style.width = '100%';
        videoElement.style.height = '100%';
        videoElement.style.backgroundColor = '#000';
        container.appendChild(videoElement);

        currentTorrent = client.add(magnetUri, {
            announce: [
                'wss://tracker.btorrent.xyz',
                'wss://tracker.openwebtorrent.com',
                'wss://tracker.fastcast.nz'
            ]
        }, (torrent: any) => {
            const file = torrent.files.find((f: any) => {
                return /\.(mp4|mkv|webm)$/i.test(f.name);
            });

            if (!file) {
                throw new Error('No video file found');
            }

            file.getBlobURL((err: any, url: string) => {
                if (!err && url) {
                    videoElement.src = url;
                    const playPromise = videoElement.play();
                    if (playPromise) {
                        playPromise.catch(() => {
                            videoElement.addEventListener('click', () => {
                                videoElement.play();
                            });
                        });
                    }
                }
            });

            let lastProgress = 0;
            torrent.on('download', () => {
                const progress = Math.floor(torrent.progress * 100);
                if (progress > lastProgress) {
                    lastProgress = progress;
                    progressElement.textContent = `Downloading: ${progress}%`;
                }

                if (videoElement.paused && torrent.progress > 0.005) {
                    videoElement.play().catch(() => {
                    });
                }
            });

            torrent.on('done', () => {
                progressElement.textContent = 'Download complete';
                if (videoElement.paused) {
                    videoElement.play().catch(console.error);
                }
            });
        });

    } catch (error) {
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