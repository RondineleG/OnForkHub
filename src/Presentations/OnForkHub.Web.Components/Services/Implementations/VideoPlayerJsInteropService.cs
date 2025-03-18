using OnForkHub.Web.Components.Services.Interfaces;
using IJSObjectReference = Microsoft.JSInterop.IJSObjectReference;

namespace OnForkHub.Web.Components.Services.Implementations;

public class VideoPlayerJsInteropService(IJSRuntime jsRuntime) : IAsyncDisposable, IVideoPlayerJsInteropService
{
    private readonly Lazy<Task<IJSObjectReference>> mainTask = new(
        () => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/main.js").AsTask()
    );

    private readonly Lazy<Task<IJSObjectReference>> moduleTask = new(
        () => jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/plyr.js").AsTask()
    );

    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    public async Task Initialize(
        string id,
        DotNetObjectReference<Player> objectRef,
        string magnetUri,
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
        await moduleTask.Value;

        if (!string.IsNullOrEmpty(magnetUri))
        {
            // Inicializa o player de torrent
            await (await mainTask.Value).InvokeVoidAsync("initTorrentPlayer", id);

            // Inicia o download do torrent
            await (await mainTask.Value).InvokeVoidAsync("startDownload", id, "#" + id, magnetUri);
        }
        else
        {
            // Inicializa o player normal
            await (await mainTask.Value).InvokeVoidAsync(
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
}
