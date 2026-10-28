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
    session.stop();
}

export function startDownload(magnetUri, containerId) {
    return new Promise((resolve, reject) => {
        const container = document.getElementById(containerId);
        if (!container) return reject(new Error('Video container not found.'));
        if (activeDownloads.has(magnetUri)) return reject(new Error('Torrent already loading or playing.'));

        const torrentClient = getClient();
        let torrent = null;
        let settled = false;
        let timeout = null;
        let stopped = false;
        const removeTorrent = () => {
            try { torrentClient.remove(magnetUri, () => {}); }
            catch (error) { console.warn('Could not remove torrent:', error); }
        };
        const clearPending = () => {
            clearTimeout(timeout);
            torrent?.removeListener?.('error', fail);
        };
        const fail = (error) => {
            if (settled || stopped) return;
            settled = true;
            clearPending();
            activeDownloads.delete(magnetUri);
            removeTorrent();
            reject(error instanceof Error ? error : new Error(String(error)));
        };
        const stop = () => {
            if (stopped) return;
            stopped = true;
            clearPending();
            activeDownloads.delete(magnetUri);
            removeTorrent();
            if (!settled) {
                settled = true;
                reject(new Error('Torrent playback cancelled.'));
            }
        };
        activeDownloads.set(magnetUri, { stop });
        timeout = setTimeout(() => fail(new Error('Timed out loading torrent metadata.')), 15000);

        try {
            torrent = torrentClient.add(magnetUri, (readyTorrent) => {
                if (stopped || settled) return;
                const file = readyTorrent.files.find(item => /\.(mp4|webm)$/i.test(item.name));
                if (!file) return fail(new Error('Torrent contains no playable MP4 or WebM video.'));
                try {
                    file.appendTo(container, error => {
                        if (stopped || settled) return;
                        if (error) return fail(error);
                        settled = true;
                        clearPending();
                        resolve();
                    });
                } catch (error) { fail(error); }
            });
            torrent?.on?.('error', fail);
        } catch (error) { fail(error); }
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
    for (const magnetUri of [...activeDownloads.keys()]) stopDownload(magnetUri);
    if (client) {
        const current = client;
        client = null;
        current.destroy((error) => {
            if (error) console.warn('WebTorrent cleanup failed:', error);
        });
    }
}
