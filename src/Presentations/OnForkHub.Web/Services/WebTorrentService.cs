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
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
    /// <param name="videoData">The video byte array.</param>
    /// <param name="fileName">The file name.</param>
    /// <returns>The magnet URI.</returns>
    public async Task<string> CreateTorrentAsync(byte[] videoData, string fileName)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<string>("createTorrent", videoData, fileName);
    }

    /// <summary>
    /// Starts downloading a torrent from a magnet URI.
    /// </summary>
    /// <param name="magnetUri">The magnet URI.</param>
    /// <param name="containerId">The HTML container ID.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task StartDownloadAsync(string magnetUri, string containerId)
    {
        await InitializeAsync();
        await _module!.InvokeVoidAsync("startDownload", magnetUri, containerId);
    }

    /// <summary>
    /// Gets stats for a specific torrent.
    /// </summary>
    /// <param name="magnetUri">The magnet URI.</param>
    /// <returns>P2P stats.</returns>
    public async Task<P2PStats?> GetTorrentStatsAsync(string magnetUri)
    {
        await InitializeAsync();
        return await _module!.InvokeAsync<P2PStats?>("getTorrentStats", magnetUri);
    }

    /// <summary>
    /// Updates the WebTorrent client configuration (e.g. bandwidth limits).
    /// </summary>
    /// <param name="maxDownloadSpeed">Max download speed in bytes/sec (-1 for unlimited).</param>
    /// <param name="maxUploadSpeed">Max upload speed in bytes/sec (-1 for unlimited).</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task UpdateConfigAsync(long maxDownloadSpeed, long maxUploadSpeed)
    {
        await InitializeAsync();
        await _module!.InvokeVoidAsync("updateConfig", maxDownloadSpeed, maxUploadSpeed);
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
