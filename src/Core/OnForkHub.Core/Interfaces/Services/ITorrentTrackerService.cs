namespace OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service for managing torrent seeding and health.
/// </summary>
public interface ITorrentTrackerService
{
    /// <summary>
    /// Gets the peer count for a specific torrent.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> GetPeerCountAsync(string magnetUri);

    /// <summary>
    /// Checks if a torrent is healthy (has seeds).
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> IsHealthyAsync(string magnetUri);

    /// <summary>
    /// Reannounces a torrent to the trackers.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task ReannounceAsync(string magnetUri);

    /// <summary>
    /// Gets detailed stats for a torrent.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<TorrentHealthStats> GetStatsAsync(string magnetUri);
}

/// <summary>
/// Health statistics for a torrent.
/// </summary>
public class TorrentHealthStats
{
    public int PeerCount { get; set; }
    public int SeedCount { get; set; }
    public int LeechCount { get; set; }
    public double HealthScore { get; set; }
}
