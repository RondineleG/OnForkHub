import type { TorrentFile, Torrent } from "webtorrent";

declare global {
    interface Window {
        WebTorrent: any;
        handleTorrentFile: (containerId: string) => Promise<void>;
        torrentCallbacks: {
            [key: string]: {
                onProgress?: (progress: number) => void;
                onReady?: () => void;
                onError?: (error: string) => void;
            }
        };
    }
}

let client: any = null;
let currentTorrent: any = null;

window.torrentCallbacks = {};

export async function initTorrentPlayer(containerId: string): Promise<void> {
    try {
        const progressElement = document.getElementById(containerId);
        if (!window.WebTorrent) throw new Error("WebTorrent not loaded in window");

        if (!client) {
            client = new window.WebTorrent();
        }

        if (progressElement) {
            progressElement.innerHTML = `
                <div style="display: flex; align-items: center; justify-content: center; height: 400px; color: white;">
                    <span>Pronto para reproduzir torrents</span>
                </div>
            `;
        }
    } catch (error) {
        console.error('Erro ao inicializar WebTorrent:', error);
        throw error;
    }
}

export async function startDownload(
    containerId: string,
    videoContainerSelector: string,
    torrentSource: string | ArrayBuffer,
    dotNetObjectRef?: any
): Promise<void> {
    try {
        if (!client) {
            client = new window.WebTorrent();
        }

        if (currentTorrent) {
            currentTorrent.destroy();
            currentTorrent = null;
        }

        const container = document.querySelector(videoContainerSelector) as HTMLElement;
        if (!container) throw new Error('Container não encontrado');

        container.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: center; width: 100%; height: 400px; background: #000; color: #fff;">
                <div style="text-align: center;">
                    <div style="margin-bottom: 10px;">Carregando torrent...</div>
                    <div id="progress-${containerId}" style="font-size: 14px;">0%</div>
                </div>
            </div>
        `;

        console.log('Adicionando torrent...');

        client.add(torrentSource, (torrent: Torrent) => {
            console.log('Torrent adicionado:', torrent.name);

            const videoFiles = torrent.files.filter((f: TorrentFile) =>
                /\.(mp4|mkv|webm|avi|mov|m4v|flv|wmv)$/i.test(f.name)
            );

            if (videoFiles.length === 0) {
                const errorMsg = 'Nenhum arquivo de vídeo encontrado no torrent.';
                container.innerHTML = `
                    <div style="display: flex; align-items: center; justify-content: center; height: 400px; color: red; background: #000;">
                        <div style="text-align: center; padding: 20px;">
                            <div>${errorMsg}</div>
                            <div style="margin-top: 10px; font-size: 12px;">Arquivos encontrados: ${torrent.files.map((f: TorrentFile) => f.name).join(', ')}</div>
                        </div>
                    </div>
                `;
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', errorMsg);
                }
                return;
            }

            const videoFile = videoFiles.reduce((a: TorrentFile, b: TorrentFile) =>
                a.length > b.length ? a : b
            );

            console.log('Arquivo de vídeo selecionado:', videoFile.name, `(${formatBytes(videoFile.length)})`);

            container.innerHTML = `
                <video 
                    controls 
                    playsinline 
                    style="width:100%; height:100%; background:#000;"
                    preload="metadata">
                    Seu navegador não suporta o elemento de vídeo.
                </video>
            `;

            const videoElement = container.querySelector('video') as HTMLVideoElement;
            if (!videoElement) {
                const errorMsg = 'Erro ao criar elemento de vídeo';
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', errorMsg);
                }
                return;
            }

            try {
                videoFile.renderTo(videoElement, {
                    autoplay: false,
                    controls: true
                }, (err: Error | undefined, element: HTMLMediaElement) => {             
                    if (err) {
                        console.error('Erro ao renderizar vídeo:', err);
                        const errorMsg = `Erro ao reproduzir vídeo: ${err.message}`;
                        container.innerHTML = `
                <div style="display: flex; align-items: center; justify-content: center; height: 400px; color: red; background: #000;">
                    <div style="text-align: center; padding: 20px;">${errorMsg}</div>
                </div>
            `;
                        if (dotNetObjectRef) {
                            dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', errorMsg);
                        }
                        return;
                    }

                    if (element instanceof HTMLVideoElement) {
                        element.muted = false;
                    }

                    console.log('Vídeo renderizado com sucesso');
                    if (dotNetObjectRef) {
                        dotNetObjectRef.invokeMethodAsync('OnTorrentReadyCallback');
                    }
                });
            } catch (renderError) {
                console.error('Erro no renderTo:', renderError);
                const errorMsg = `Erro ao inicializar reprodução: ${renderError}`;
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', errorMsg);
                }
            }

            torrent.on('download', () => {
                const progress = Math.floor(torrent.progress * 100);
                const speed = formatBytes(torrent.downloadSpeed);
                const downloaded = formatBytes(torrent.downloaded);
                const total = formatBytes(torrent.length);

                const progressElement = document.getElementById(`progress-${containerId}`);
                if (progressElement) {
                    progressElement.textContent = `${progress}% - ${downloaded}/${total} - ${speed}/s`;
                }

                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('OnTorrentProgressUpdate', progress);
                }
            });

            torrent.on('done', () => {
                console.log('Download completo');
                const progressElement = document.getElementById(`progress-${containerId}`);
                if (progressElement) {
                    progressElement.textContent = 'Download completo';
                }
            });

            torrent.on('error', (err: Error | String) => {
                console.error('Erro no torrent:', err);
                const errorMsg = err instanceof Error ? err.message : err;;
                if (dotNetObjectRef) {
                    dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', errorMsg);
                }
            });

            currentTorrent = torrent;
        });

    } catch (error) {
        console.error('Erro em startDownload:', error);
        const errorMsg = error instanceof Error ? error.message : 'Erro desconhecido';
        if (dotNetObjectRef) {
            dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', errorMsg);
        }
        throw error;
    }
}

export async function startDownloadFromFile(
    containerId: string,
    videoContainerSelector: string,
    dotNetObjectRef?: any
): Promise<void> {
    return new Promise((resolve, reject) => {
        const fileInput = document.getElementById(`torrent-file-${containerId}`) as HTMLInputElement;
        if (!fileInput) {
            const error = 'Input de arquivo não encontrado';
            if (dotNetObjectRef) {
                dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', error);
            }
            reject(new Error(error));
            return;
        }

        const file = fileInput.files?.[0];
        if (!file) {
            const error = 'Nenhum arquivo selecionado';
            if (dotNetObjectRef) {
                dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', error);
            }
            reject(new Error(error));
            return;
        }

        if (!file.name.endsWith('.torrent')) {
            const error = 'Arquivo deve ter extensão .torrent';
            if (dotNetObjectRef) {
                dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', error);
            }
            reject(new Error(error));
            return;
        }

        const reader = new FileReader();
        reader.onload = async (e) => {
            try {
                const arrayBuffer = e.target?.result as ArrayBuffer;
                if (!arrayBuffer) {
                    const error = 'Erro ao ler arquivo';
                    if (dotNetObjectRef) {
                        dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', error);
                    }
                    reject(new Error(error));
                    return;
                }

                await startDownload(
                    containerId,
                    videoContainerSelector,
                    arrayBuffer,
                    dotNetObjectRef
                );
                resolve();
            } catch (error) {
                reject(error);
            }
        };

        reader.onerror = () => {
            const error = 'Erro ao ler arquivo .torrent';
            if (dotNetObjectRef) {
                dotNetObjectRef.invokeMethodAsync('OnTorrentErrorCallback', error);
            }
            reject(new Error(error));
        };

        reader.readAsArrayBuffer(file);
    });
}

export async function stopDownload(): Promise<void> {
    try {
        if (currentTorrent) {
            currentTorrent.destroy();
            currentTorrent = null;
        }

        if (client) {
            await new Promise<void>((resolve) => {
                client.destroy(() => {
                    client = new window.WebTorrent();
                    resolve();
                });
            });
        }
    } catch (error) {
        console.error('Erro ao parar download:', error);
        throw error;
    }
}

window.handleTorrentFile = async function (containerId: string): Promise<void> {
    try {
        const callbacks = window.torrentCallbacks[containerId];
        await startDownloadFromFile(containerId, `#${containerId}`, callbacks);
    } catch (error) {
        console.error('Erro ao carregar arquivo torrent:', error);
    }
};

function formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
}
