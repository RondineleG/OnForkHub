interface WebTorrentInstance {
    add(magnetUri: string, opts: any, callback: (torrent: Torrent) => void): void;

    destroy(): void;
}

interface TorrentFile {
    name: string;
    length: number;

    getBlobURL(callback: (err: string | Error | undefined, blobURL?: string) => void): void;
}

interface TorrentEvents {
    download: (bytes: number) => void;
    upload: (bytes: number) => void;
    done: () => void;
    error: (err: Error) => void;
    warning: (err: Error | string) => void;
    ready: () => void;
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

export async function initTorrentPlayer(progressElement: HTMLElement): Promise<void> {
    try {
        progressElement.innerHTML = 'Initializing WebTorrent...';
        if (!client) {
            client = new window.WebTorrent();
        }
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
        client = new window.WebTorrent();
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

            client!.add(magnetUri, torrentOpts, (torrent) => {
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

                videoFile.getBlobURL((err, url) => {
                    if (err) {
                        console.error('Error getting blob URL:', err);
                        reject(err);
                        return;
                    }

                    if (!url) {
                        reject(new Error('No blob URL received'));
                        return;
                    }

                    console.log('Got blob URL, setting up video...');
                    video.src = url;
                    video.style.display = 'block';

                    // Remove loading UI quando o v�deo estiver pronto
                    const loadingUI = containerElement.querySelector('.d-flex');
                    if (loadingUI) {
                        containerElement.removeChild(loadingUI);
                    }

                    // Espera metadados carregarem antes de tentar reproduzir
                    video.addEventListener('loadedmetadata', () => {
                        console.log('Video metadata loaded');
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

                torrent.on('error', (err) => {
                    console.error('Torrent error:', err);
                    reject(err);
                });

                torrent.on('warning', (err) => {
                    if (err instanceof Error && !err.message.includes('tracker')) {
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