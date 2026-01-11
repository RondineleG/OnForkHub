namespace OnForkHub.Web.Components.Services.Interfaces;

public interface IVideoPlayerJsInteropService
{
    Task CleanupTorrent();

    Task Initialize(
        string id,
        DotNetObjectReference<Player> objectRef,
        string magnetUri,
        string torrentFilePath,
        bool enableTorrentFileUpload,
        bool captions,
        bool quality,
        bool speed,
        bool loop,
        bool playLargeControl,
        bool restartControl,
        bool rewindControl,
        bool playControl,
        bool fastForwardControl,
        bool progressControl,
        bool currentTimeControl,
        bool durationControl,
        bool muteControl,
        bool volumeControl,
        bool captionsControl,
        bool settingsControl,
        bool pipControl,
        bool airplayControl,
        bool downloadControl,
        bool fullscreenControl
    );

    Task StartTorrentFromFile(string elementId, DotNetObjectReference<Player> objectRef);
}
