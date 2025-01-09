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

        // Create video element
        const video = document.createElement('video');
        video.style.width = '100%';
        video.style.height = '100%';
        video.style.maxHeight = '480px';
        video.controls = true;

        // Clear container and add video
        videoContainer.innerHTML = '';
        videoContainer.appendChild(video);

        return new Promise((resolve, reject) => {
            client.add(magnetUri, (torrent: any) => {
                // Get the largest video file
                const files = torrent.files.filter((file: any) =>
                    file.name.endsWith('.mp4') ||
                    file.name.endsWith('.webm') ||
                    file.name.endsWith('.mkv')
                );
                const file = files.reduce((a: any, b: any) => a.length > b.length ? a : b);

                if (!file) {
                    reject(new Error('No video file found'));
                    return;
                }

                // Stream to video element
                file.renderTo(video);

                torrent.on('download', () => {
                    const progress = (torrent.progress * 100).toFixed(1);
                    progressElement.textContent = `Downloading: ${progress}%`;
                });

                torrent.on('done', () => {
                    progressElement.textContent = 'Download complete';
                    resolve();
                });
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