let players = {};

export function initializePlayer(playerId, manifestUrl, autoPlay, statsId) {
    const video = document.getElementById(playerId);
    if (!video) return;

    const player = dashjs.MediaPlayer().create();
    player.initialize(video, manifestUrl, autoPlay);
    
    // Task 2.2.7: Auto-Quality is enabled by default in dash.js
    player.updateSettings({
        'streaming': {
            'abr': {
                'autoSwitchBitrate': {
                    'video': true
                }
            }
        }
    });

    players[playerId] = player;

    // Update stats periodically
    const statsInterval = setInterval(() => {
        const statsElement = document.getElementById(statsId);
        if (!statsElement) {
            clearInterval(statsInterval);
            return;
        }

        const bitrates = player.getBitrateInfoListFor('video');
        const currentId = player.getQualityFor('video');
        const currentBitrate = bitrates[currentId];
        
        if (currentBitrate) {
            const kbps = Math.round(currentBitrate.bitrate / 1000);
            const resolution = `${currentBitrate.width}x${currentBitrate.height}`;
            statsElement.innerText = `Qualidade: ${resolution} (${kbps} kbps)`;
        }
    }, 2000);
}

export function destroyPlayer(playerId) {
    if (players[playerId]) {
        players[playerId].reset();
        delete players[playerId];
    }
}
