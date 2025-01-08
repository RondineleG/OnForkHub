declare const WebTorrent: any;

let client: any = null;

export async function initTorrentPlayer(progressElement: HTMLElement): Promise<void> {
    try {
        client = new WebTorrent();
        progressElement.textContent = "WebTorrent initialized successfully";
    } catch (error) {
        console.error('Error initializing WebTorrent:', error);
        throw error;
    }
}

export async function startDownload(
    progressElement: HTMLElement,
    videoContainerSelector: string,
    magnetUri: string
): Promise<void> {
    try {
        const videoContainer = document.querySelector(videoContainerSelector);
        if (!videoContainer) {
            throw new Error('Video container not found');
        }

        if (!client) {
            throw new Error('WebTorrent not initialized');
        }

        videoContainer.innerHTML = '';
        const videoElement = document.createElement('video');
        videoElement.controls = true;
        videoElement.className = 'w-100 h-100';
        videoContainer.appendChild(videoElement);
        client.add(magnetUri, {
            announce: [
                'wss://tracker.openwebtorrent.com',
                'wss://tracker.btorrent.xyz',
                'wss://tracker.fastcast.nz'
            ]
        }, (torrent: any) => {
            const file = torrent.files.find((f: any) =>
                f.name.endsWith('.mp4') ||
                f.name.endsWith('.webm') ||
                f.name.endsWith('.mkv')
            );

            if (!file) {
                throw new Error('No video file found in torrent');
            }

            file.getBlobURL((err: any, url: string) => {
                if (err) throw err;
                if (!url) throw new Error('Failed to get video URL');
                videoElement.src = url;
            });

            torrent.on('download', () => {
                const percent = (torrent.progress * 100).toFixed(1);
                progressElement.textContent = `Downloading: ${percent}%`;
                progressElement.className = 'alert alert-info';
            });
        });
    } catch (error) {
        console.error('Error starting download:', error);
        throw error;
    }
}

export async function stopDownload(): Promise<void> {
    if (client) {
        client.destroy(() => {
            client = new WebTorrent();
        });
    }
}