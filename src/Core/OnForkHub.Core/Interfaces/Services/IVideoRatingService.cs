namespace OnForkHub.Core.Interfaces.Services;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Service contract for video rating operations.
/// </summary>
public interface IVideoRatingService
{
    /// <summary>
    /// Sets a user's rating for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult<VideoRating>> SetRatingAsync(Guid videoId, Id userId, ERatingType type);

    /// <summary>
    /// Removes a user's rating for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult> RemoveRatingAsync(Guid videoId, Id userId);

    /// <summary>
    /// Gets rating statistics for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult<VideoRatingStats>> GetStatsAsync(Guid videoId, Id? userId = null);
}
