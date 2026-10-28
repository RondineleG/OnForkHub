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
        
        getClient().seed(file, (torrent) => {
            console.log('Client is seeding:', torrent.magnetURI);
            resolve(torrent.magnetURI);
        });
    });
}

export function startDownload(magnetUri, containerId) {
    return new Promise((resolve, reject) => {
        const container = document.getElementById(containerId);
        if (!container) {
            reject(new Error('Video container not found.'));
            return;
        }

        let settled = false;
        let torrent;
        let timeout;
        const cleanup = () => {
            clearTimeout(timeout);
            torrent?.removeListener('error', fail);
        };
        const fail = (error) => {
            if (settled) return;
            settled = true;
            cleanup();
            reject(error instanceof Error ? error : new Error(String(error)));
        };
        const succeed = () => {
            if (settled) return;
            settled = true;
            cleanup();
            resolve();
        };

        timeout = setTimeout(() => fail(new Error('Timed out loading torrent metadata.')), 15000);
        try {
            torrent = getClient().add(magnetUri, (readyTorrent) => {
                if (settled) return;
                const file = readyTorrent.files.find((item) => /\\.(mp4|webm)$/i.test(item.name));
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
