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

        client.add(magnetUri, { announce: ['wss://tracker.webtorrent.io'] }, (torrent: any) => {
            const files = torrent.files;
            const videoFile = files.find((file: any) => {
                return /\.(mp4|mkv|webm)$/i.test(file.name);
            });

            if (!videoFile) {
                throw new Error('No video file found in torrent');
            }

            videoFile.getBlobURL((err: Error | null, url: string) => {
                if (err) throw err;
                videoElement.src = url;
                videoElement.play();
            });

            let lastProgress = 0;
            torrent.on('download', () => {
                const newProgress = Math.floor(torrent.progress * 100);
                if (newProgress > lastProgress) {
                    lastProgress = newProgress;
                    progressElement.textContent = `Downloading: ${newProgress}%`;
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