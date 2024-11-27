import WebTorrent from 'webtorrent';

const clients: { [key: string]: WebTorrent.Instance } = {};

interface Player {
    invokeMethodAsync(methodName: string, ...args: any[]): Promise<void>;
}

export async function initTorrentPlayer(
    elementId: string,
    component: Player,
    torrentId: string
): Promise<void> {
    try {
        clients[elementId] = new WebTorrent();
        const client = clients[elementId];

        const container = document.getElementById(elementId);
        if (!container) {
            throw new Error(`Container #${elementId} not found`);
        }

        const video = container.querySelector('video') as HTMLVideoElement;
        const progress = container.querySelector('.torrent-progress') as HTMLElement;

        if (!video || !progress) {
            throw new Error('Required elements not found');
        }

        // Setup event listeners
        video.addEventListener('play', () => {
            component.invokeMethodAsync('OnPlay');
        });

        video.addEventListener('ended', () => {
            component.invokeMethodAsync('OnEnded');
        });

        video.addEventListener('timeupdate', () => {
            component.invokeMethodAsync('OnTimeUpdate',
                video.currentTime,
                video.duration
            );
        });

        // Add torrent
        client.add(torrentId, {
            announce: [
                'wss://tracker.openwebtorrent.com',
                'wss://tracker.btorrent.xyz',
                'wss://tracker.fastcast.nz'
            ]
        }, torrent => {
            const file = torrent.files.find(f =>
                f.name.endsWith('.mp4') ||
                f.name.endsWith('.webm') ||
                f.name.endsWith('.mkv')
            );

            if (!file) {
                throw new Error('No video file found in torrent');
            }

            file.getBlobURL((err, url) => {
                if (err) throw err;
                if (!url) throw new Error('Failed to get video URL');

                video.src = url;
            });

            torrent.on('download', () => {
                const percent = (torrent.progress * 100).toFixed(1);
                progress.textContent = `Downloading: ${percent}%`;
                progress.style.display = torrent.progress === 1 ? 'none' : 'block';
            });
        });

    } catch (error) {
        console.error('Error initializing torrent player:', error);
        throw error;
    }
}

export function disposeTorrentPlayer(elementId: string): void {
    const client = clients[elementId];
    if (client) {
        client.destroy();
        delete clients[elementId];
    }
}