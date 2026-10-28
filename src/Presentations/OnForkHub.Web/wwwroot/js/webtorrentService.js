let client = null;

function getClient() {
    if (!client) {
        client = new WebTorrent();
    }
    return client;
}

export function createTorrent(videoData, fileName) {
    return new Promise((resolve, reject) => {
        const blob = new Blob([videoData], { type: 'video/mp4' });
        const file = new File([blob], fileName);
        
        const torrentClient = getClient();
        torrentClient.seed(file, (torrent) => {
            console.log('Client is seeding:', torrent.magnetURI);
            resolve(torrent.magnetURI);
        });
    });
}

const activeDownloads = new Map();

export function stopDownload(magnetUri) {
    const session = activeDownloads.get(magnetUri);
    if (!session) return;
    activeDownloads.delete(magnetUri);
    session.cancel();
    try {
        getClient().remove(magnetUri, () => {});
    } catch (error) {
        console.warn('Could not remove torrent:', error);
    }
}

export function startDownload(magnetUri, containerId) {
    return new Promise((resolve, reject) => {
        const container = document.getElementById(containerId);
        if (!container) {
            reject(new Error('Video container not found.'));
            return;
        }
        if (activeDownloads.has(magnetUri)) {
            reject(new Error('Torrent already loading or playing.'));
            return;
        }

        let settled = false;
        let torrent = null;
        let timeout;
        const cleanup = () => {
            clearTimeout(timeout);
            torrent?.removeListener('error', fail);
        };
        const fail = (error) => {
            if (settled) return;
            settled = true;
            cleanup();
            activeDownloads.delete(magnetUri);
            try { getClient().remove(magnetUri, () => {}); } catch { /* Not registered yet. */ }
            reject(error instanceof Error ? error : new Error(String(error)));
        };
        const succeed = () => {
            if (settled) return;
            settled = true;
            cleanup();
            resolve();
        };
        activeDownloads.set(magnetUri, {
            cancel: () => fail(new Error('Torrent playback cancelled.'))
        });
        timeout = setTimeout(() => fail(new Error('Timed out loading torrent metadata.')), 15000);

        try {
            torrent = getClient().add(magnetUri, (readyTorrent) => {
                if (settled) return;
                const file = readyTorrent.files.find((item) => /\.(mp4|webm)$/i.test(item.name));
                if (!file) {
                    fail(new Error('Torrent contains no playable MP4 or WebM video.'));
                    return;
                }
                try {
                    file.appendTo(container, (error) => error ? fail(error) : succeed());
                } catch (error) {
                    fail(error);
                }
            });
            torrent.on('error', fail);
        } catch (error) {
            fail(error);
        }
    });
}

export function getTorrentStats(magnetUri) {
    const torrent = getClient().get(magnetUri);
    if (!torrent) return null;

    return {
        peers: torrent.numPeers,
        progress: torrent.progress * 100,
        downloadSpeed: torrent.downloadSpeed,
        uploadSpeed: torrent.uploadSpeed
    };
}

export function updateConfig(maxDownloadSpeed, maxUploadSpeed) {
    // WebTorrent client-level throttling
    getClient().throttleDownload(maxDownloadSpeed || -1);
    getClient().throttleUpload(maxUploadSpeed || -1);
}


// Explicitly release downloads, seed connections and browser resources when the player shuts down.
export function disposeWebTorrent() {
    for (const [magnetUri, session] of activeDownloads) {
        session.cancel();
        activeDownloads.delete(magnetUri);
    }
    if (client) {
        const current = client;
        client = null;
        current.destroy((error) => {
            if (error) console.warn('WebTorrent cleanup failed:', error);
        });
    }
}
