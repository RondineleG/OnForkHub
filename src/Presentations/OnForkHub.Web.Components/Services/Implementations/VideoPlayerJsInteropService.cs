using OnForkHub.Web.Components.Services.Interfaces;

namespace OnForkHub.Web.Components.Services.Implementations;

public class VideoPlayerJsInteropService(IJSRuntime jsRuntime) : IAsyncDisposable, IVideoPlayerJsInteropService
{
    private readonly Lazy<Task<IJSObjectReference>> _mainTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/main.js").AsTask()
    );

    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/plyr.js").AsTask()
    );

    public async ValueTask DisposeAsync()
    {
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
            // Carrega os módulos
            await _moduleTask.Value;
            var mainModule = await _mainTask.Value;

            // Verifica se é para usar WebTorrent
            var useTorrent = !string.IsNullOrEmpty(magnetUri) ||
                           !string.IsNullOrEmpty(torrentFilePath) ||
                           enableTorrentFileUpload;

            if (useTorrent)
            {
                // Inicializa WebTorrent
                await mainModule.InvokeVoidAsync("initTorrentPlayer", id);

                // Configura callback global para upload de arquivo
                if (enableTorrentFileUpload)
                {
                    var script = $@"
                         if (!window.torrentCallbacks) window.torrentCallbacks = {{}};
                         window.torrentCallbacks['{id}'] = {{
                             invokeMethodAsync: function(method, ...args) {{
                                 return DotNet.invokeMethodAsync('{objectRef.GetType().Assembly.GetName().Name}', method, ...args);
                             }}
                         }};

                         window.handleTorrentFile_{id} = async function() {{
                             try {{
                                 const mainModule = await import('./_content/OnForkHub.Web.Components/main.js');
                                 await mainModule.startDownloadFromFile('{id}', '#{id}', window.torrentCallbacks['{id}']);
                             }} catch (error) {{
                                 console.error('Erro ao carregar torrent:', error);
                                 window.torrentCallbacks['{id}'].invokeMethodAsync('OnTorrentErrorCallback', error.message);
                             }}
                         }};
                      ";

                    await jsRuntime.InvokeVoidAsync("eval", script);
                }

                // Se tem magnet URI, inicia download automaticamente
                if (!string.IsNullOrEmpty(magnetUri))
                {
                    await mainModule.InvokeVoidAsync("startDownload", id, "#" + id, magnetUri, objectRef);
                }
            }
            else
            {
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
}
