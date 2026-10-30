// Torrent playback on top of the WebTorrent global loaded from the CDN (0.98.x).
// Files are streamed with renderTo(): MP4/WebM start playing as soon as the first pieces arrive,
// instead of waiting for the whole download like getBlobURL() does.

interface RenderOptions {
    autoplay?: boolean;
    controls?: boolean;
}

interface TorrentFile {
    name: string;
    length: number;
    renderTo(element: HTMLVideoElement, opts: RenderOptions, callback: (err: Error | null) => void): void;
}

interface TorrentEvents {
    done: () => void;
    error: (err: Error | string) => void;
    warning: (err: Error | string) => void;
}

interface Torrent {
    files: TorrentFile[];
    progress: number;
    downloadSpeed: number;
    numPeers: number;
    on<K extends keyof TorrentEvents>(event: K, callback: TorrentEvents[K]): void;
    destroy(): void;
}

interface WebTorrentOptions {
    tracker?: { rtcConfig?: RTCConfiguration };
}

interface WebTorrentInstance {
    add(source: string | File, opts: object, callback: (torrent: Torrent) => void): Torrent;
    destroy(): void;
}

interface DotNetRef {
    invokeMethodAsync(method: string, ...args: unknown[]): Promise<void>;
}

declare global {
    interface Window {
        WebTorrent: new (opts?: WebTorrentOptions) => WebTorrentInstance;
    }
}

const TRACKERS = ['wss://tracker.openwebtorrent.com', 'wss://tracker.webtorrent.dev'];

// WebTorrent 0.98 defaults to 'stun:...?transport=udp', which current browsers reject, so every WebRTC peer
// connection would fail and only HTTP web seeds would work.
const RTC_CONFIG = {
    iceServers: [{ urls: ['stun:stun.l.google.com:19302', 'stun:global.stun.twilio.com:3478'] }],
};
const PLAYABLE_VIDEO = /\.(mp4|m4v|webm|mkv)$/i;
const STATS_INTERVAL_MS = 500;

let client: WebTorrentInstance | null = null;
let currentTorrent: Torrent | null = null;
let statsTimer: number | null = null;

function getClient(): WebTorrentInstance {
    if (!window.WebTorrent) {
        throw new Error('WebTorrent library not loaded');
    }
    client ??= new window.WebTorrent({ tracker: { rtcConfig: RTC_CONFIG } });
    return client;
}

function releaseCurrentTorrent(): void {
    if (statsTimer !== null) {
        window.clearInterval(statsTimer);
        statsTimer = null;
    }
    currentTorrent?.destroy();
    currentTorrent = null;
}

function showMessage(container: Element, text: string): void {
    container.innerHTML = `
        <div class="d-flex flex-column align-items-center justify-content-center text-white-50" style="height: 400px;">
            <div class="spinner-border mb-3" role="status" aria-hidden="true"></div>
            <span></span>
        </div>`;
    container.querySelector('span')!.textContent = text;
}

function reportError(dotNetRef: DotNetRef | undefined, error: unknown): void {
    const message = error instanceof Error ? error.message : String(error);
    console.error('Torrent error:', message);
    void dotNetRef?.invokeMethodAsync('OnTorrentErrorCallback', message);
}

function reportStats(dotNetRef: DotNetRef | undefined, torrent: Torrent): void {
    void dotNetRef?.invokeMethodAsync(
        'OnTorrentStatsUpdate',
        Math.floor(torrent.progress * 100),
        torrent.numPeers,
        torrent.downloadSpeed
    );
}

// The largest playable file is the movie; torrents often ship subtitles, posters or samples alongside it.
function findVideoFile(torrent: Torrent): TorrentFile | undefined {
    return torrent.files.filter((file) => PLAYABLE_VIDEO.test(file.name)).sort((a, b) => b.length - a.length)[0];
}

function playTorrent(source: string | File, containerSelector: string, dotNetRef?: DotNetRef): void {
    const container = document.querySelector(containerSelector);
    if (!container) {
        reportError(dotNetRef, `Container element not found: ${containerSelector}`);
        return;
    }

    releaseCurrentTorrent();
    showMessage(container, 'Conectando aos peers...');

    try {
        const torrent = getClient().add(source, { announce: TRACKERS, maxWebConns: 8 }, (readyTorrent) => {
            const videoFile = findVideoFile(readyTorrent);
            if (!videoFile) {
                reportError(dotNetRef, 'O torrent não contém um vídeo MP4, WebM ou MKV.');
                return;
            }

            showMessage(container, `Carregando ${videoFile.name}...`);

            const video = document.createElement('video');
            video.className = 'w-100 d-block';
            video.style.maxHeight = '70vh';
            video.style.background = '#000';
            video.playsInline = true;

            videoFile.renderTo(video, { autoplay: true, controls: true }, (err) => {
                if (err) {
                    reportError(dotNetRef, err);
                }
            });

            video.addEventListener(
                'loadedmetadata',
                () => {
                    container.replaceChildren(video);
                    void dotNetRef?.invokeMethodAsync('OnTorrentReadyCallback', videoFile.name);
                },
                { once: true }
            );
        });

        currentTorrent = torrent;
        statsTimer = window.setInterval(() => reportStats(dotNetRef, torrent), STATS_INTERVAL_MS);

        torrent.on('done', () => reportStats(dotNetRef, torrent));
        torrent.on('error', (err) => {
            releaseCurrentTorrent();
            reportError(dotNetRef, err);
        });
        torrent.on('warning', (err) => console.warn('Torrent warning:', err));
    } catch (error) {
        releaseCurrentTorrent();
        reportError(dotNetRef, error);
    }
}

export async function initTorrentPlayer(_elementId: string): Promise<void> {
    getClient();
}

export async function startDownload(
    _elementId: string,
    containerSelector: string,
    magnetUri: string,
    dotNetRef?: DotNetRef
): Promise<void> {
    playTorrent(magnetUri, containerSelector, dotNetRef);
}

export async function startDownloadFromFile(elementId: string, containerSelector: string, dotNetRef?: DotNetRef): Promise<void> {
    const fileInput = document.getElementById(`torrent-file-${elementId}`) as HTMLInputElement | null;
    const file = fileInput?.files?.[0];
    if (!file) {
        reportError(dotNetRef, 'Nenhum arquivo .torrent selecionado.');
        return;
    }

    playTorrent(file, containerSelector, dotNetRef);
}

export function stopDownload(): void {
    releaseCurrentTorrent();
    client?.destroy();
    client = null;
}
