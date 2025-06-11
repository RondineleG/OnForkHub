interface WebTorrentInstance {
    add(magnetUri: string | File, opts: any, callback: (torrent: Torrent) => void): void;
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
let currentVideo: HTMLVideoElement | null = null;

function assertIsHTMLElement(element: Element | null): asserts element is HTMLElement {
    if (!element || !(element instanceof HTMLElement)) {
        throw new Error('Element is not an HTMLElement');
    }
}

function cleanupCurrentStream(): void {
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
        <div class="w-100 h-100 d-flex flex-column align-items-center justify-content-center" style="height: 400px;">
            <div class="loading-status text-white mb-3">Initializing...</div>
            <div class="progress w-75">
                <div class="progress-bar bg-primary" role="progressbar" style="width: 0%; height: 20px;"></div>
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
    video.style.maxHeight = '100%';
    video.style.maxWidth = '100%';
    return video;
}

export async function initTorrentPlayer(elementId: string): Promise<void> {
    try {
        console.log('Initializing WebTorrent for element:', elementId);
        if (!window.WebTorrent) {
            throw new Error('WebTorrent library not loaded');
        }

        if (!client) {
            client = new window.WebTorrent();
            console.log('WebTorrent client created');
        }
    } catch (error) {
        console.error('Error initializing WebTorrent:', error);
        throw error;
    }
}

export async function startDownload(
    elementId: string,
    containerSelector: string,
    magnetUri: string,
    dotNetRef?: any
): Promise<void> {
    console.log('Starting download for element:', elementId);

    cleanupCurrentStream();

    if (!client) {
        console.log('Client not initialized, setting up...');
        await initTorrentPlayer(elementId);
    }

    if (currentTorrent) {
        console.log('Destroying existing torrent...');
        currentTorrent.destroy();
        currentTorrent = null;
    }

    return new Promise<void>((resolve, reject) => {
        try {
            const containerElement = document.querySelector(containerSelector);
            if (!containerElement) {
                throw new Error(`Container element not found: ${containerSelector}`);
            }

            containerElement.innerHTML = createLoadingUI();

            const loadingStatus = containerElement.querySelector('.loading-status');
            const progressBar = containerElement.querySelector('.progress-bar');

            if (loadingStatus) assertIsHTMLElement(loadingStatus);
            if (progressBar) assertIsHTMLElement(progressBar);

            const torrentOpts = {
                announce: [
                    'wss://tracker.openwebtorrent.com',
                    'wss://tracker.webtorrent.dev',
                    'wss://tracker.btorrent.xyz'
                ],
                maxWebConns: 8,
                strategy: 'sequential'
            };

            client!.add(magnetUri, torrentOpts, (torrent) => {
                console.log('Torrent added, files:', torrent.files.length);
                currentTorrent = torrent;

                const videoFile = torrent.files.find((file) => {
                    const name = file.name.toLowerCase();
                    const isVideo = name.endsWith('.mp4') ||
                        name.endsWith('.webm') ||
                        name.endsWith('.mkv') ||
                        name.endsWith('.avi') ||
                        name.endsWith('.mov') ||
                        name.endsWith('.wmv') ||
                        name.endsWith('.flv');
                    console.log('Checking file:', file.name, 'isVideo:', isVideo);
                    return isVideo;
                });

                if (!videoFile) {
                    const error = 'No video file found in torrent';
                    console.error(error);
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
                    }
                    reject(new Error(error));
                    return;
                }

                console.log('Video file found:', videoFile.name);

                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnTorrentReadyCallback');
                }

                const video = createVideoElement();
                currentVideo = video;
                containerElement.appendChild(video);

                videoFile.getBlobURL((err, url) => {
                    if (err) {
                        console.error('Error getting blob URL:', err);
                        if (dotNetRef) {
                            dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', err.toString());
                        }
                        reject(err);
                        return;
                    }

                    if (!url) {
                        const error = 'No blob URL received';
                        console.error(error);
                        if (dotNetRef) {
                            dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
                        }
                        reject(new Error(error));
                        return;
                    }

                    console.log('Got blob URL, setting up video...');
                    video.src = url;
                    video.style.display = 'block';

                    // Remove loading UI
                    const loadingUI = containerElement.querySelector('.d-flex');
                    if (loadingUI) {
                        loadingUI.remove();
                    }

                    video.addEventListener('loadedmetadata', () => {
                        console.log('Video metadata loaded');
                        if (torrent.progress > 0.05) {
                            video.play().catch(playError => {
                                console.warn('Could not auto-start video:', playError);
                            });
                        }
                    });

                    video.addEventListener('error', (e) => {
                        console.error('Video error:', e);
                        if (dotNetRef) {
                            dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', 'Video playback error');
                        }
                    });
                });

                torrent.on('download', () => {
                    const progress = Math.round(torrent.progress * 100);
                    //const speed = (torrent.downloadSpeed / (1024 * 1024)).toFixed(2);

                    //console.log(`Progress: ${progress}%, Speed: ${speed} MB/s`);

                    if (loadingStatus) {
                        loadingStatus.textContent = `Loading: ${progress}%`;
                    }
                    if (progressBar) {
                        progressBar.style.width = `${progress}%`;
                    }

                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentProgressUpdate', progress);
                    }

                    if (torrent.progress > 0.05 && currentVideo && currentVideo.paused) {
                        currentVideo.play().catch(playError => {
                            console.warn('Could not start video:', playError);
                        });
                    }
                });

                torrent.on('done', () => {
                    console.log('Download complete');
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentProgressUpdate', 100);
                    }
                    resolve();
                });

                torrent.on('error', (err) => {
                    console.error('Torrent error:', err);
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', err.message);
                    }
                    reject(err);
                });

                torrent.on('warning', (err) => {
                    console.warn('Torrent warning:', err);
                });
            });
        } catch (error) {
            console.error('Error in startDownload:', error);
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
            }
            reject(error);
        }
    });
}

export async function startDownloadFromFile(
    elementId: string,
    containerSelector: string,
    dotNetRef?: any
): Promise<void> {
    const fileInput = document.getElementById(`torrent-file-${elementId}`) as HTMLInputElement;
    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        const error = 'No file selected';
        console.error(error);
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
        }
        return;
    }

    const file = fileInput.files[0];
    console.log('Starting download from file:', file.name);

    cleanupCurrentStream();

    if (!client) {
        console.log('Client not initialized, setting up...');
        await initTorrentPlayer(elementId);
    }

    if (currentTorrent) {
        console.log('Destroying existing torrent...');
        currentTorrent.destroy();
        currentTorrent = null;
    }

    return new Promise<void>((resolve, reject) => {
        try {
            const containerElement = document.querySelector(containerSelector);
            if (!containerElement) {
                throw new Error(`Container element not found: ${containerSelector}`);
            }

            containerElement.innerHTML = createLoadingUI();

            const loadingStatus = containerElement.querySelector('.loading-status');
            const progressBar = containerElement.querySelector('.progress-bar');

            if (loadingStatus) assertIsHTMLElement(loadingStatus);
            if (progressBar) assertIsHTMLElement(progressBar);

            const torrentOpts = {
                announce: [
                    'wss://tracker.openwebtorrent.com',
                    'wss://tracker.webtorrent.dev',
                    'wss://tracker.btorrent.xyz'
                ],
                maxWebConns: 8,
                strategy: 'sequential'
            };

            client!.add(file, torrentOpts, (torrent) => {
                console.log('Torrent added from file, files:', torrent.files.length);
                currentTorrent = torrent;

                const videoFile = torrent.files.find((file) => {
                    const name = file.name.toLowerCase();
                    const isVideo = name.endsWith('.mp4') ||
                        name.endsWith('.webm') ||
                        name.endsWith('.mkv') ||
                        name.endsWith('.avi') ||
                        name.endsWith('.mov') ||
                        name.endsWith('.wmv') ||
                        name.endsWith('.flv');
                    return isVideo;
                });

                if (!videoFile) {
                    const error = 'No video file found in torrent';
                    console.error(error);
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
                    }
                    reject(new Error(error));
                    return;
                }

                console.log('Video file found:', videoFile.name);

                if (dotNetRef) {
                    dotNetRef.invokeMethodAsync('OnTorrentReadyCallback');
                }

                const video = createVideoElement();
                currentVideo = video;
                containerElement.appendChild(video);

                videoFile.getBlobURL((err, url) => {
                    if (err) {
                        console.error('Error getting blob URL:', err);
                        if (dotNetRef) {
                            dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', err.toString());
                        }
                        reject(err);
                        return;
                    }

                    if (!url) {
                        const error = 'No blob URL received';
                        if (dotNetRef) {
                            dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
                        }
                        reject(new Error(error));
                        return;
                    }

                    console.log('Got blob URL, setting up video...');
                    video.src = url;
                    video.style.display = 'block';

                    // Remove loading UI
                    const loadingUI = containerElement.querySelector('.d-flex');
                    if (loadingUI) {
                        loadingUI.remove();
                    }

                    video.addEventListener('loadedmetadata', () => {
                        console.log('Video metadata loaded');
                        if (torrent.progress > 0.05) {
                            video.play().catch(playError => {
                                console.warn('Could not auto-start video:', playError);
                            });
                        }
                    });
                });

                // Mesmo sistema de eventos do método anterior
                torrent.on('download', () => {
                    const progress = Math.round(torrent.progress * 100);
                    /*  const speed = (torrent.downloadSpeed / (1024 * 1024)).toFixed(2);*/

                    if (loadingStatus) {
                        loadingStatus.textContent = `Loading: ${progress}%`;
                    }
                    if (progressBar) {
                        progressBar.style.width = `${progress}%`;
                    }

                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentProgressUpdate', progress);
                    }

                    if (torrent.progress > 0.05 && currentVideo && currentVideo.paused) {
                        currentVideo.play().catch(playError => {
                            console.warn('Could not start video:', playError);
                        });
                    }
                });

                torrent.on('done', () => {
                    console.log('Download complete');
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentProgressUpdate', 100);
                    }
                    resolve();
                });

                torrent.on('error', (err) => {
                    console.error('Torrent error:', err);
                    if (dotNetRef) {
                        dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', err.message);
                    }
                    reject(err);
                });
            });
        } catch (error) {
            console.error('Error in startDownloadFromFile:', error);
            if (dotNetRef) {
                dotNetRef.invokeMethodAsync('OnTorrentErrorCallback', error);
            }
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

    if (client) {
        client.destroy();
        client = null;
    }

    console.log('Download stopped, resources cleaned up');
}
