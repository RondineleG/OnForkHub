interface WebTorrentInstance {
    add(magnetUri: string, opts: any, callback: (torrent: any) => void): void;
    destroy(): void;
}

interface TorrentEvents {
    download: (bytes: number) => void;
    upload: (bytes: number) => void;
    done: () => void;
    error: (err: Error) => void;
    warning: (err: Error) => void;
    ready: () => void;
}

interface TorrentFile {
    name: string;
    length: number;
    getBlobURL(callback: (err: Error | null, url: string) => void): void;
    createReadStream(opts?: { start: number; end: number }): any;
}

interface Torrent {
    files: TorrentFile[];
    progress: number;
    downloadSpeed: number;
    on<K extends keyof TorrentEvents>(event: K, callback: TorrentEvents[K]): void;
    destroy(): void;
}

declare global {
    interface Window {
        WebTorrent: new () => WebTorrentInstance;
    }
}

let client: WebTorrentInstance | null = null;
let currentTorrent: Torrent | null = null;
let initialized = false;
let currentStream: any = null;
let currentVideo: HTMLVideoElement | null = null;

function assertIsHTMLElement(element: Element | null): asserts element is HTMLElement {
    if (!element || !(element instanceof HTMLElement)) {
        throw new Error('Element is not an HTMLElement');
    }
}

function cleanupCurrentStream(): void {
    if (currentStream) {
        try {
            currentStream.destroy();
        } catch (e) {
            console.warn('Error destroying stream:', e);
        }
        currentStream = null;
    }

    if (currentVideo) {
        try {
            const oldSrc = currentVideo.src;
            currentVideo.pause();
            currentVideo.src = '';
            currentVideo.load();
            if (oldSrc.startsWith('blob:')) {
                URL.revokeObjectURL(oldSrc);
            }
        } catch (e) {
            console.warn('Error cleaning up video element:', e);
        }
        currentVideo = null;
    }
}

function createLoadingUI(): string {
    return `
        <div class="w-100 h-100 d-flex flex-column align-items-center justify-content-center">
            <div class="loading-status text-white mb-3">Initializing...</div>
            <div class="progress w-75">
                <div class="progress-bar" role="progressbar" style="width: 0%"></div>
            </div>
        </div>
    `;
}

function createVideoElement(): HTMLVideoElement {
    const video = document.createElement('video');
    video.style.display = 'none';
    video.className = 'w-100 h-100';
    video.style.objectFit = 'contain';
    video.playsInline = true;
    video.controls = true;
    video.preload = 'auto';
    return video;
}

async function loadWebTorrentScript(): Promise<void> {
    if (document.querySelector('script[src*="webtorrent"]')) {
        return;
    }

    return new Promise((resolve, reject) => {
        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/webtorrent@latest/webtorrent.min.js';
        script.async = true;

        script.onload = () => {
            console.log('WebTorrent script loaded');
            resolve();
        };

        script.onerror = (error) => {
            console.error('Failed to load WebTorrent script:', error);
            reject(new Error('Failed to load WebTorrent script'));
        };

        document.head.appendChild(script);
    });
}

async function ensureWebTorrent(): Promise<void> {
    if (!initialized) {
        await loadWebTorrentScript();
        if (window.WebTorrent) {
            console.log('Creating WebTorrent client...');
            client = new window.WebTorrent();
            initialized = true;
            console.log('WebTorrent client created');
        } else {
            throw new Error('WebTorrent not loaded');
        }
    }
}

export async function initTorrentPlayer(progressElement: HTMLElement): Promise<void> {
    try {
        progressElement.innerHTML = 'Initializing WebTorrent...';
        await ensureWebTorrent();
        progressElement.innerHTML = 'WebTorrent initialized';
    } catch (error) {
        const errorMessage = error instanceof Error ? error.message : 'Unknown error';
        progressElement.innerHTML = `Error: ${errorMessage}`;
        throw error;
    }
}

export async function startDownload(
    progressElement: HTMLElement,
    container: string,
    magnetUri: string
): Promise<void> {
    console.log('Starting download...');

    cleanupCurrentStream();

    if (!client) {
        console.log('Client not initialized, setting up...');
        await ensureWebTorrent();
    }

    if (currentTorrent) {
        console.log('Destroying existing torrent...');
        currentTorrent.destroy();
        currentTorrent = null;
    }

    return new Promise<void>((resolve, reject) => {
        try {
            const containerElement = document.querySelector(container);
            if (!containerElement) {
                throw new Error(`Container element not found: ${container}`);
            }

            containerElement.innerHTML = createLoadingUI();

            const loadingStatus = containerElement.querySelector('.loading-status');
            const progressBar = containerElement.querySelector('.progress-bar');

            if (loadingStatus) assertIsHTMLElement(loadingStatus);
            if (progressBar) assertIsHTMLElement(progressBar);

            const torrentOpts = {
                announce: [
                    'wss://tracker.openwebtorrent.com',
                    'wss://tracker.webtorrent.dev'
                ],
                maxWebConns: 4,
                strategy: 'sequential'
            };

            client!.add(magnetUri, torrentOpts, (torrent: Torrent) => {
                console.log('Torrent added, searching for video file...');
                currentTorrent = torrent;

                const videoFile = torrent.files.find((file) => {
                    const isVideo = file.name.endsWith('.mp4') ||
                        file.name.endsWith('.webm') ||
                        file.name.endsWith('.mkv');
                    console.log('Checking file:', file.name, 'isVideo:', isVideo);
                    return isVideo;
                });

                if (!videoFile) {
                    throw new Error('No video file found in torrent');
                }

                console.log('Video file found:', videoFile.name);

                const video = createVideoElement();
                currentVideo = video;
                containerElement.appendChild(video);

                videoFile.getBlobURL((error: Error | null, url: string) => {
                    if (error) {
                        console.error('Error getting blob URL:', error);
                        reject(error);
                        return;
                    }

                    console.log('Got blob URL, setting up video...');
                    video.src = url;
                    video.style.display = 'block';

                    // Remove loading UI quando o vídeo estiver pronto
                    const loadingUI = containerElement.querySelector('.d-flex');
                    if (loadingUI) {
                        containerElement.removeChild(loadingUI);
                    }

                    // Espera metadados carregarem antes de tentar reproduzir
                    video.addEventListener('loadedmetadata', () => {
                        console.log('Video metadata loaded');
                        // Só tenta reproduzir se tivermos dados suficientes
                        if (torrent.progress > 0.1) {
                            video.play().catch(playError => {
                                console.warn('Could not auto-start video:', playError);
                            });
                        }
                    });
                });

                torrent.on('download', () => {
                    const progress = (torrent.progress * 100).toFixed(1);
                    const speed = (torrent.downloadSpeed / (1024 * 1024)).toFixed(2);
                    const status = `Progress: ${progress}% (${speed} MB/s)`;

                    if (loadingStatus) {
                        loadingStatus.textContent = `Loading: ${progress}%`;
                    }
                    if (progressBar) {
                        progressBar.style.width = `${progress}%`;
                    }

                    progressElement.innerHTML = status;

                    // Tenta iniciar o vídeo se tiver dados suficientes
                    if (torrent.progress > 0.1 && currentVideo && currentVideo.paused) {
                        currentVideo.play().catch(playError => {
                            console.warn('Could not start video:', playError);
                        });
                    }
                });

                torrent.on('done', () => {
                    console.log('Download complete');
                    progressElement.innerHTML = 'Download complete';
                    resolve();
                });

                torrent.on('error', (err: Error) => {
                    console.error('Torrent error:', err);
                    reject(err);
                });

                torrent.on('warning', (err: Error) => {
                    if (!err.message.includes('tracker')) {
                        console.warn('Torrent warning:', err);
                    }
                });
            });
        } catch (error) {
            console.error('Error in startDownload:', error);
            reject(error);
        }
    });
}

export function stopDownload(): void {
    console.log('Stopping download...');

    cleanupCurrentStream();

    if (currentTorrent) {
        currentTorrent.destroy();
        currentTorrent = null;
    }

    const container = document.querySelector('#videoContainer');
    if (container) {
        container.innerHTML = '';
    }

    console.log('Download stopped, resources cleaned up');
}