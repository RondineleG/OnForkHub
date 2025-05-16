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
            <video controls playsinline style="width:100%; height:100%; background:#000;"></video>
        `;

        const videoElement = container.querySelector('video');
        if (!videoElement) throw new Error('Video element not found');

        client.add(magnetUri, (torrent: any) => {
            const files = torrent.files.filter((f: any) => {
                return /\.(mp4|mkv|webm)$/i.test(f.name);
            });
            const videoFile = files.reduce((a: any, b: any) => a.length > b.length ? a : b);

            videoFile.renderTo(videoElement, {
                autoplay: true,
                controls: true,
                muted: false
            });

            torrent.on('download', () => {
                const progress = Math.floor(torrent.progress * 100);
                progressElement.textContent = `Downloading: ${progress}%`;
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