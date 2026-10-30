// WebTorrent 2.x, vendored in lib/webtorrent together with its service worker (/webtorrent-sw.min.js, same version).
// Files are streamed through the worker: file.streamTo(video) points video.src at a URL the worker answers with
// torrent pieces, so the browser's native player handles every format it supports, including seeking.
const LIBRARY_URL = '/lib/webtorrent/webtorrent.min.js';
const WORKER_URL = '/webtorrent-sw.min.js';
const LOAD_TIMEOUT_MS = 30000;

let client = null;
let clientPromise = null;

async function loadWebTorrent() {
    // A host page (or a test) may already provide the constructor globally.
    return globalThis.WebTorrent ?? (await import(LIBRARY_URL)).default;
}

async function registerStreamWorker() {
    const container = globalThis.navigator?.serviceWorker;
    if (!container) throw new Error('Torrent streaming needs service workers, which require HTTPS (or localhost).');
    const registration = await container.register(WORKER_URL, { scope: '/' });
    const worker = registration.active ?? registration.waiting ?? registration.installing;
    if (worker && worker.state !== 'activated') {
        await new Promise((resolve) => {
            worker.addEventListener('statechange', function onStateChange() {
                if (worker.state !== 'activated') return;
                worker.removeEventListener('statechange', onStateChange);
                resolve();
            });
        });
    }
    // The worker claims open pages when it activates; streams only work once it controls this one.
    if (!container.controller) {
        await new Promise((resolve) => container.addEventListener('controllerchange', resolve, { once: true }));
    }
    return registration;
}

function getClient() {
    clientPromise ??= (async () => {
        const WebTorrent = await loadWebTorrent();
        const instance = new WebTorrent();
        instance.createServer({ controller: await registerStreamWorker() });
        client = instance;
        return instance;
    })().catch((error) => {
        clientPromise = null;
        throw error;
    });
    return clientPromise;
}

export async function createTorrent(videoData, fileName) {
    const blob = new Blob([videoData], { type: 'video/mp4' });
    const file = new File([blob], fileName);
    const torrentClient = await getClient();

    return new Promise((resolve) => {
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

// `magnetUri` may also be an http(s) URL to a .torrent file. Resolves with the <video> element appended to the
// container once it can play. `options`: autoplay and controls (default true), muted (default false) and
// preload (default 'auto').
export function startDownload(magnetUri, containerId, options = {}) {
    return new Promise((resolve, reject) => {
        const container = document.getElementById(containerId);
        if (!container) return reject(new Error('Video container not found.'));
        if (activeDownloads.has(magnetUri)) return reject(new Error('Torrent already loading or playing.'));

        let torrent = null;
        let video = null;
        let settled = false;
        let timeout = null;
        let stopped = false;
        // Destroying the torrent object works for every source; client.remove() cannot resolve a .torrent URL.
        const removeTorrent = () => {
            try {
                if (typeof torrent?.destroy === 'function') torrent.destroy();
                else client?.remove(magnetUri, () => {});
            }
            catch (error) { console.warn('Could not remove torrent:', error); }
        };
        const onCanPlay = () => {
            if (stopped || settled) return;
            settled = true;
            clearPending();
            resolve(video);
        };
        const onVideoError = () => fail(new Error(video?.error?.message || 'The browser cannot play this video.'));
        const clearPending = () => {
            clearTimeout(timeout);
            torrent?.removeListener?.('error', fail);
            video?.removeEventListener('canplay', onCanPlay);
            video?.removeEventListener('error', onVideoError);
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
        activeDownloads.set(magnetUri, { stop, get torrent() { return torrent; } });
        timeout = setTimeout(() => fail(new Error('Timed out waiting for the torrent video to become playable.')), LOAD_TIMEOUT_MS);

        getClient().then((torrentClient) => {
            if (stopped || settled) return;
            torrent = torrentClient.add(magnetUri, (readyTorrent) => {
                if (stopped || settled) return;
                // The largest playable file is the movie; torrents often ship samples or extras alongside it.
                const file = readyTorrent.files
                    .filter(item => /\.(mp4|m4v|webm)$/i.test(item.name))
                    .sort((a, b) => (b.length ?? 0) - (a.length ?? 0))[0];
                if (!file) return fail(new Error('Torrent contains no playable MP4 or WebM video.'));

                video = document.createElement('video');
                video.autoplay = options.autoplay ?? true;
                video.controls = options.controls ?? true;
                video.muted = options.muted ?? false;
                video.preload = options.preload ?? 'auto';
                video.playsInline = true;
                video.addEventListener('canplay', onCanPlay);
                video.addEventListener('error', onVideoError);
                container.appendChild(video);
                try { file.streamTo(video); }
                catch (error) { fail(error); }
            });
            torrent?.on?.('error', fail);
        }).catch(fail);
    });
}

// Stats for a session started with startDownload, or null when it is not active.
export function getTorrentStats(magnetUri) {
    const torrent = activeDownloads.get(magnetUri)?.torrent;
    if (!torrent) return null;

    return {
        peers: torrent.numPeers,
        progress: torrent.progress * 100,
        downloadSpeed: torrent.downloadSpeed,
        uploadSpeed: torrent.uploadSpeed
    };
}

export async function updateConfig(maxDownloadSpeed, maxUploadSpeed) {
    // WebTorrent client-level throttling
    const torrentClient = await getClient();
    torrentClient.throttleDownload(maxDownloadSpeed || -1);
    torrentClient.throttleUpload(maxUploadSpeed || -1);
}

// Explicitly release downloads, seed connections and browser resources when the player shuts down.
export function disposeWebTorrent() {
    for (const magnetUri of [...activeDownloads.keys()]) stopDownload(magnetUri);
    clientPromise = null;
    if (client) {
        const current = client;
        client = null;
        current.destroy((error) => {
            if (error) console.warn('WebTorrent cleanup failed:', error);
        });
    }
}
