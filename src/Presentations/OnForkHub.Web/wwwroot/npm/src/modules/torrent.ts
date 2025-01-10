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

        const video = document.createElement('video');
        video.style.width = '100%';
        video.style.height = '100%';
        video.style.maxHeight = '480px';
        video.controls = true;

        videoContainer.innerHTML = '';
        videoContainer.appendChild(video);

        client.add(magnetUri, (torrent: any) => {
            const file = torrent.files.find((file: any) =>
                file.name.endsWith('.mp4') ||
                file.name.endsWith('.webm') ||
                file.name.endsWith('.mkv')
            );

            if (!file) throw new Error('No video file found');

            // Start streaming immediately
            const stream = file.createReadStream();
            let buffer = [];
            let isPlaying = false;

            stream.on('data', (chunk: any) => {
                buffer.push(chunk);
                if (!isPlaying && buffer.length > 10) {
                    isPlaying = true;
                    file.getBlobURL((err: any, url: string) => {
                        if (!err && url) {
                            video.src = url;
                            video.play();
                        }
                    });
                }
            });

            torrent.on('download', () => {
                const progress = (torrent.progress * 100).toFixed(1);
                progressElement.textContent = `Downloading: ${progress}%`;

                if (torrent.progress > 0.05 && !isPlaying) {
                    isPlaying = true;
                    file.getBlobURL((err: any, url: string) => {
                        if (!err && url) {
                            video.src = url;
                            video.play();
                        }
                    });
                }
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