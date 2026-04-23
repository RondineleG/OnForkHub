namespace OnForkHub.Web.Services;

/// <summary>
/// P2P statistics for a torrent.
/// </summary>
public class P2PStats
{
    /// <summary>
    /// Gets or sets the number of peers.
    /// </summary>
    public int Peers { get; set; }

    /// <summary>
    /// Gets or sets the progress percentage.
    /// </summary>
    public double Progress { get; set; }

    /// <summary>
    /// Gets or sets the download speed in bytes per second.
    /// </summary>
    public double DownloadSpeed { get; set; }

    /// <summary>
    /// Gets or sets the upload speed in bytes per second.
    /// </summary>
    public double UploadSpeed { get; set; }
}
