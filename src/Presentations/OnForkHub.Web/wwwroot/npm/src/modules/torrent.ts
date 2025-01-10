declare const WebTorrent: any;
let client: any = null;

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

        const container = document.querySelector(videoContainerSelector) as HTMLElement;
        const videoElement = document.createElement('video');
        videoElement.controls = true;
        videoElement.style.width = '100%';
        videoElement.style.height = '100%';
        videoElement.style.backgroundColor = '#000';
        container.innerHTML = '';
        container.appendChild(videoElement);

        client.add(magnetUri, {
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

            file.renderTo(videoElement, {
                autoplay: true,
                muted: false
            });

            torrent.on('download', () => {
                const progress = Math.floor(torrent.progress * 100);
                progressElement.textContent = `Downloading: ${progress}%`;
                if (torrent.progress > 0.01 && videoElement.paused) {
                    videoElement.play().catch(console.error);
                }
            });

            torrent.on('done', () => {
                progressElement.textContent = 'Download complete';
            });
        });

    } catch (error) {
        throw error;
    }
}

export async function stopDownload(): Promise<void> {
    if (client) {
        await new Promise<void>((resolve) => {
            client.destroy(() => {
                client = new WebTorrent();
                resolve();
            });
        });
    }
}