using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OnForkHub.Web.Components;

public partial class TorrentPlayer : ComponentBase, IAsyncDisposable
{
    [Parameter]
    public string PlayerId { get; set; } = "torrent-player";

    [Parameter]
    [EditorRequired]
    public string TorrentId { get; set; }

    [Parameter]
    public EventCallback OnEndedVideo { get; set; }

    [Parameter]
    public EventCallback OnPlayVideo { get; set; }

    [Parameter]
    public EventCallback<(float currentTime, float duration)> OnVideoTimeUpdate { get; set; }

    private DotNetObjectReference<TorrentPlayer> _objectRef;
    private IJSObjectReference _moduleRef;

    protected override void OnInitialized()
    {
        _objectRef = DotNetObjectReference.Create(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            try
            {
                _moduleRef = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/OnForkHub.Web.Components/js/main.min.js");

                // Corrigido o formato dos parâmetros
                var parameters = new object[] { PlayerId, _objectRef, TorrentId };
                await _moduleRef.InvokeVoidAsync("initTorrentPlayer", parameters);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error initializing torrent player: {ex.Message}");
                throw;
            }
        }
    }

    [JSInvokable]
    public async Task OnEnded()
    {
        await OnEndedVideo.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnPlay()
    {
        await OnPlayVideo.InvokeAsync();
    }

    [JSInvokable]
    public async Task OnTimeUpdate(float currentTime, float duration)
    {
        await OnVideoTimeUpdate.InvokeAsync((currentTime, duration));
    }

    // Corrigido a implementação do IAsyncDisposable
    public virtual async ValueTask DisposeAsync()
    {
        try
        {
            if (_moduleRef is not null)
            {
                await _moduleRef.InvokeVoidAsync("disposeTorrentPlayer", PlayerId);
                await _moduleRef.DisposeAsync();
            }

            _objectRef?.Dispose();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error disposing torrent player: {ex.Message}");
        }
    }
}
