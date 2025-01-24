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

        const videoElement = document.createElement('video');
        videoElement.style.width = '100%';
        videoElement.style.height = '100%';
        videoElement.style.backgroundColor = '#000';
        videoElement.controls = true;
        videoElement.autoplay = true;
        container.appendChild(videoElement);

        currentTorrent = client.add(magnetUri, {
            announce: [
                'wss://tracker.btorrent.xyz',
                'wss://tracker.openwebtorrent.com',
                'wss://tracker.fastcast.nz'
            ]
        });

        currentTorrent.on('ready', () => {
            const file = currentTorrent.files.find((f: any) => {
                return /\.(mp4|mkv|webm)$/i.test(f.name);
            });

            if (!file) {
                throw new Error('No video file found');
            }

            file.getBlobURL((err: any, url: string) => {
                if (err) throw err;

                videoElement.src = url;
                const playPromise = videoElement.play();
                if (playPromise) {
                    playPromise.catch(() => {
                        console.log("Autoplay prevented, trying again...");
                        file.createReadStream().pipe(videoElement);          
                    });
                }
            });

            file.select();
        });

        currentTorrent.on('download', () => {
            const progress = Math.floor(currentTorrent.progress * 100);
            progressElement.textContent = `Downloading: ${progress}%`;

            if (videoElement.paused) {
                videoElement.play().catch(console.error);
            }
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