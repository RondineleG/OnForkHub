using OnForkHub.Web.Components.Services.Interfaces;

namespace OnForkHub.Web.Components.Services.Implementations;

public class VideoPlayerJsInteropService(IJSRuntime jsRuntime) : IAsyncDisposable, IVideoPlayerJsInteropService
{
    private readonly Lazy<Task<IJSObjectReference>> _mainTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/js/main.min.js").AsTask()
    );

    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/plyr.js").AsTask()
    );

    public async Task CleanupTorrent()
    {
        try
        {
            var mainModule = await _mainTask.Value;
            await mainModule.InvokeVoidAsync("stopDownload");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao limpar torrent: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await CleanupTorrent();
        }
        catch { }

        if (_mainTask.IsValueCreated)
        {
            var mainModule = await _mainTask.Value;
            await mainModule.DisposeAsync();
        }

        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task Initialize(
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
        bool pIPControl,
        bool airplayControl,
        bool downloadControl,
        bool fullscreenControl
    )
    {
        try
        {
            var mainModule = await _mainTask.Value;

            // Verifica se é para usar WebTorrent
            var useTorrent = !string.IsNullOrEmpty(magnetUri) || !string.IsNullOrEmpty(torrentFilePath) || enableTorrentFileUpload;

            if (useTorrent)
            {
                // Inicializa WebTorrent
                await mainModule.InvokeVoidAsync("initTorrentPlayer", id);

                // Se tem magnet URI, inicia download automaticamente
                if (!string.IsNullOrEmpty(magnetUri))
                {
                    await mainModule.InvokeVoidAsync("startDownload", id, $"#{id}", magnetUri, objectRef);
                }
            }
            else
            {
                // Carrega o módulo Plyr se necessário
                await _moduleTask.Value;

                // Inicialização normal do player de vídeo (Plyr)
                await mainModule.InvokeVoidAsync(
                    "videoInitialize",
                    id,
                    objectRef,
                    captions,
                    quality,
                    speed,
                    loop,
                    playLargeControl,
                    restartControl,
                    rewindControl,
                    playControl,
                    fastForwardControl,
                    progressControl,
                    currentTimeControl,
                    durationControl,
                    muteControl,
                    volumeControl,
                    captionsControl,
                    settingsControl,
                    pIPControl,
                    airplayControl,
                    downloadControl,
                    fullscreenControl
                );
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro na inicialização do player: {ex.Message}");
            throw;
        }
    }

    public async Task StartTorrentFromFile(string elementId, DotNetObjectReference<Player> objectRef)
    {
        try
        {
            var mainModule = await _mainTask.Value;
            await mainModule.InvokeVoidAsync("startDownloadFromFile", elementId, $"#{elementId}", objectRef);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao iniciar torrent do arquivo: {ex.Message}");
            throw;
        }
    }
}
