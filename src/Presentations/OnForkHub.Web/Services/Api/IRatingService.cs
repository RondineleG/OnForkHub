namespace OnForkHub.Web.Services.Api;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Requests.Videos;

/// <summary>
/// Service contract for video rating API operations.
/// </summary>
public interface IRatingService
{
    /// <summary>
    /// Sets a rating for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> SetRatingAsync(Guid videoId, ERatingType type);

    /// <summary>
    /// Removes a user's rating for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<bool> RemoveRatingAsync(Guid videoId);

    /// <summary>
    /// Gets rating statistics for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<VideoRatingStats> GetStatsAsync(Guid videoId);
}

/// <summary>
/// Statistics for video ratings.
/// </summary>
public class VideoRatingStats
{
    public int Likes { get; set; }
    public int Dislikes { get; set; }
    public ERatingType? UserRating { get; set; }
}
