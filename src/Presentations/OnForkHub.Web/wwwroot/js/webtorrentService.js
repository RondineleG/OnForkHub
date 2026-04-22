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
    getClient().add(magnetUri, (torrent) => {
        console.log('Client is downloading:', torrent.infoHash);
        
        // Render the video in the container
        torrent.files.forEach((file) => {
            if (file.name.endsWith('.mp4') || file.name.endsWith('.webm')) {
                file.appendTo(`#${containerId}`);
            }
        });
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
