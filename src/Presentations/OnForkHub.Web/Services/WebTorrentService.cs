namespace OnForkHub.Web.Services;

using Microsoft.JSInterop;

/// <summary>
/// Service for interacting with WebTorrent.js via JS Interop.
/// </summary>
public sealed class WebTorrentService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    /// <summary>
    /// Initializes a new instance of the <see cref="WebTorrentService"/> class.
    /// </summary>
    /// <param name="jsRuntime">The JS runtime.</param>
    public WebTorrentService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Initializes the WebTorrent module.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task InitializeAsync()
    {
        if (_module == null)
        {
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/webtorrentService.js");
        }
    }

    /// <summary>
    /// Creates a torrent from video data.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task<string> CreateTorrentAsync(byte[] videoData, string fileName)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<string>("createTorrent", videoData, fileName);
    }

    /// <summary>
    /// Starts downloading a torrent from a magnet URI.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task StartDownloadAsync(string magnetUri, string containerId)
    {
        await InitializeAsync();
        await _module!.InvokeVoidAsync("startDownload", magnetUri, containerId);
    }

    /// <summary>
    /// Gets stats for a specific torrent.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public async Task<P2PStats?> GetTorrentStatsAsync(string magnetUri)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<P2PStats?>("getTorrentStats", magnetUri);
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync()
    {
        if (_module != null)
        {
            await _module.DisposeAsync();
        }
    }
}
