import Plyr from 'plyr';
import { initTorrentPlayer, startDownload, startDownloadFromFile, stopDownload } from './modules/torrent.js';

export {
    initTorrentPlayer,
    startDownload,
    startDownloadFromFile,
    stopDownload
};

type DotNetComponent = {
    invokeMethodAsync(method: string, ...args: any[]): Promise<void>;
};

export function videoInitialize(
    elementId: string,
    component: DotNetComponent,
    captions: boolean,
    quality: boolean,
    speed: boolean,
    loop: boolean,
    playLargeControl: boolean,
    restartControl: boolean,
    rewindControl: boolean,
    playControl: boolean,
    fastForwardControl: boolean,
    progressControl: boolean,
    currentTimeControl: boolean,
    durationControl: boolean,
    muteControl: boolean,
    volumeControl: boolean,
    captionsControl: boolean,
    settingsControl: boolean,
    pIPControl: boolean,
    airplayControl: boolean,
    downloadControl: boolean,
    fullscreenControl: boolean
): void {
    const settingsArray: string[] = [];
    if (captions) settingsArray.push("captions");
    if (quality) settingsArray.push("quality");
    if (speed) settingsArray.push("speed");
    if (loop) settingsArray.push("loop");

    const controlsArray: string[] = [];
    if (playLargeControl) controlsArray.push("play-large");
    if (restartControl) controlsArray.push("restart");
    if (rewindControl) controlsArray.push("rewind");
    if (playControl) controlsArray.push("play");
    if (fastForwardControl) controlsArray.push("fast-forward");
    if (progressControl) controlsArray.push("progress");
    if (currentTimeControl) controlsArray.push("current-time");
    if (durationControl) controlsArray.push("duration");
    if (muteControl) controlsArray.push("mute");
    if (volumeControl) controlsArray.push("volume");
    if (captionsControl) controlsArray.push("captions");
    if (settingsControl) controlsArray.push("settings");
    if (pIPControl) controlsArray.push("pip");
    if (airplayControl) controlsArray.push("airplay");
    if (downloadControl) controlsArray.push("download");
    if (fullscreenControl) controlsArray.push("fullscreen");

    const player = new Plyr('#' + elementId, {
        settings: settingsArray,
        controls: controlsArray,
        quality: {
            default: 576,
            options: [4320, 2880, 2160, 1440, 1080, 720, 576, 480, 360, 240, 200, 100, 50, 20]
        }
    });

    player.on('ended', async () => {
        await component.invokeMethodAsync('OnEnded');
    });

    player.on('timeupdate', async () => {
        await component.invokeMethodAsync('OnTimeUpdate', player.currentTime, player.duration);
    });

    player.on('play', async () => {
        await component.invokeMethodAsync('OnPlay');
    });
}
