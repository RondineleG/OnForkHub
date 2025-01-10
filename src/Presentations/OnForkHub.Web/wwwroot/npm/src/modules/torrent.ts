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
        const container = document.querySelector(videoContainerSelector);
        if (!container) throw new Error('Container not found');

        container.innerHTML = `
            <video controls playsinline 
                style="width:100%;height:100%;object-fit:contain;background:#000;">
            </video>
        `;
        const video = container.querySelector('video');

        client.add(magnetUri, (torrent: any) => {
            const file = torrent.files.find((f: any) =>
                f.name.endsWith('.mp4') ||
                f.name.endsWith('.mkv') ||
                f.name.endsWith('.webm')
            );

            if (!file) throw new Error('No video file found');

            file.getBlobURL((err: any, url: string) => {
                if (err) throw err;
                if (video) {
                    video.src = url;
                    video.play().catch(console.error);
                }
            });

            torrent.on('download', () => {
                progressElement.textContent =
                    `Loading: ${(torrent.progress * 100).toFixed(1)}%`;
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