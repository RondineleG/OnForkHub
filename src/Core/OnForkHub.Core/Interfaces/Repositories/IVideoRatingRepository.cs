namespace OnForkHub.Core.Interfaces.Repositories;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;

/// <summary>
/// Repository contract for VideoRating entity.
/// </summary>
public interface IVideoRatingRepository
{
    /// <summary>
    /// Upserts a rating (creates if not exists, updates if it does).
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult<VideoRating>> SetRatingAsync(VideoRating rating);

    /// <summary>
    /// Removes a rating.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult> RemoveRatingAsync(Guid videoId, Id userId);

    /// <summary>
    /// Gets rating statistics for a video.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<RequestResult<VideoRatingStats>> GetStatsAsync(Guid videoId, Id? userId = null);
}

/// <summary>
/// Aggregated statistics for video ratings.
/// </summary>
public record VideoRatingStats(int Likes, int Dislikes, ERatingType? UserRating);
