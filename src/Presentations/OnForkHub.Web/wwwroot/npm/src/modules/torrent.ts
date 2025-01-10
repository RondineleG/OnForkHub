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
        if (!client) client = new WebTorrent();

        const videoContainer = document.querySelector(videoContainerSelector) as HTMLDivElement;
        if (!videoContainer) throw new Error('Video container not found');

        videoContainer.innerHTML = '<video controls style="width: 100%; height: 100%;"></video>';
        const video = videoContainer.querySelector('video') as HTMLVideoElement;

        client.add(magnetUri, {
            announce: [
                'wss://tracker.openwebtorrent.com',
                'wss://tracker.btorrent.xyz',
                'wss://tracker.fastcast.nz'
            ]
        }, (torrent: any) => {
            const file = torrent.files.find((file: any) =>
                file.name.endsWith('.mp4') ||
                file.name.endsWith('.webm') ||
                file.name.endsWith('.mkv')
            );

            if (!file) throw new Error('No video file found');

            file.renderTo(video, {
                autoplay: true,
                muted: false,
                controls: true,
                errorCallback: (err: any) => {
                    console.error('Error rendering:', err);
                }
            });

            torrent.on('download', () => {
                const progress = (torrent.progress * 100).toFixed(1);
                progressElement.textContent = `Downloading: ${progress}%`;
            });

            torrent.on('done', () => {
                progressElement.textContent = 'Download complete!';
            });
        });

    } catch (error) {
        throw error;
    }
}

export async function stopDownload(): Promise<void> {
    if (client) {
        client.destroy();
        client = new WebTorrent();
    }
}