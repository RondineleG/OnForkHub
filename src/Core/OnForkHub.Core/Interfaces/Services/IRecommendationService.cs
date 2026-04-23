namespace OnForkHub.Core.Interfaces.Services;

using OnForkHub.Core.Responses;

/// <summary>
/// Service for generating video recommendations for users.
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Gets a list of recommended videos for a specific user.
    /// </summary>
    /// <param name="userId">The user identifier.</param>
    /// <param name="count">The number of recommendations to return.</param>
    /// <returns>A list of video responses.</returns>
    Task<RequestResult<IEnumerable<VideoResponse>>> GetRecommendationsAsync(string userId, int count = 10);

    /// <summary>
    /// Gets trending videos for anonymous or new users.
    /// </summary>
    /// <param name="count">The number of videos to return.</param>
    /// <returns>A list of video responses.</returns>
    Task<RequestResult<IEnumerable<VideoResponse>>> GetTrendingVideosAsync(int count = 10);
}
