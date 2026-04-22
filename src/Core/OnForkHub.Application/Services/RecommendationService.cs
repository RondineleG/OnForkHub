namespace OnForkHub.Application.Services;

using Microsoft.Extensions.Logging;

using OnForkHub.Application.Services.Base;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;

/// <summary>
/// Service implementation for video recommendations.
/// </summary>
public sealed class RecommendationService(IVideoRepositoryEF videoRepository, ILogger<RecommendationService> logger)
    : BaseService,
        IRecommendationService
{
    private readonly IVideoRepositoryEF _videoRepository = videoRepository;
    private readonly ILogger<RecommendationService> _logger = logger;

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<VideoResponse>>> GetRecommendationsAsync(string userId, int count = 10)
    {
        return await ExecuteAsync(async () =>
        {
            var result = await _videoRepository.GetAllAsync(1, count);

            if (result.Status != EResultStatus.Success || result.Data == null)
            {
                return RequestResult<IEnumerable<VideoResponse>>.WithError("Failed to load recommendations");
            }

            var responses = result.Data.Select(VideoResponse.FromVideo);
            return RequestResult<IEnumerable<VideoResponse>>.Success(responses);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<VideoResponse>>> GetTrendingVideosAsync(int count = 10)
    {
        return await GetRecommendationsAsync(string.Empty, count);
    }
}
