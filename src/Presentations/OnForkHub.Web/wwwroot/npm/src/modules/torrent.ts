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
        container.innerHTML = '';

        let startedPlaying = false;

        currentTorrent = client.add(magnetUri, {
            announce: [
                'wss://tracker.btorrent.xyz',
                'wss://tracker.openwebtorrent.com',
                'wss://tracker.fastcast.nz'
            ],
            strategy: 'sequential'             
        }, (torrent: any) => {
            const file = torrent.files.find((f: any) => /\.(mp4|mkv|webm)$/i.test(f.name));
            if (!file) {
                throw new Error('No video file found');
            }

            file.streamTo({
                container: container as HTMLElement,
                autoplay: true,
                controls: true,
                muted: false,
                onReady: (video: HTMLVideoElement) => {
                    video.style.width = '100%';
                    video.style.height = '100%';
                    video.style.backgroundColor = '#000';
                    if (!startedPlaying) {
                        startedPlaying = true;
                        video.play();
                    }
                },
                onError: (err: Error) => {
                    console.error('Stream error:', err);
                }
            });

            file.select();
        });

        currentTorrent.on('download', () => {
            const progress = Math.floor(currentTorrent.progress * 100);
            progressElement.textContent = `Downloading: ${progress}%`;
        });

        currentTorrent.on('error', (err: Error) => {
            console.error('Torrent error:', err);
            throw err;
        });

    } catch (error) {
        console.error('Error in startDownload:', error);
        throw error;
    }
}

export async function stopDownload(): Promise<void> {
    try {
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
    } catch (error) {
        console.error('Error stopping download:', error);
        throw error;
    }
}