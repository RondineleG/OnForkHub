using IJSObjectReference = Microsoft.JSInterop.IJSObjectReference;

namespace OnForkHub.Web.Components.VideoPlayer;

public partial class TorrentPlayer : ComponentBase, IAsyncDisposable
{
    private IJSObjectReference? _moduleRef;

    private DotNetObjectReference<TorrentPlayer>? _objectRef;

    [Inject]
    protected IJSRuntime JSRuntime { get; set; } = default!;

    [Parameter]
    public string PlayerId { get; set; } = "torrent-player";

    [Parameter]
    [EditorRequired]
    public string TorrentId { get; set; } = default!;

    [Parameter]
    public EventCallback OnEndedVideo { get; set; }

    [Parameter]
    public EventCallback OnPlayVideo { get; set; }

    [Parameter]
    public EventCallback<(float currentTime, float duration)> OnVideoTimeUpdate { get; set; }

    public async ValueTask DisposeAsync()
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

                await _moduleRef.InvokeVoidAsync("initTorrentPlayer", [PlayerId, _objectRef, TorrentId]);
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
}