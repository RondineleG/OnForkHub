namespace OnForkHub.Web.Services.Api;

using OnForkHub.Core.Responses;

/// <summary>
/// Service contract for video recommendation API operations.
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Gets recommended videos.
    /// </summary>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task<List<VideoResponse>> GetRecommendationsAsync(int count = 10);
}
