namespace OnForkHub.Application.Services;

using Microsoft.Extensions.Logging;

using OnForkHub.Application.Services.Base;
using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service implementation for video rating operations.
/// </summary>
public sealed class VideoRatingService(IVideoRatingRepository ratingRepository, ILogger<VideoRatingService> logger) : BaseService, IVideoRatingService
{
    private readonly IVideoRatingRepository _ratingRepository = ratingRepository;
    private readonly ILogger<VideoRatingService> _logger = logger;

    /// <inheritdoc/>
    public async Task<RequestResult<VideoRating>> SetRatingAsync(Guid videoId, Id userId, ERatingType type)
    {
        return await ExecuteAsync(async () =>
        {
            var ratingResult = VideoRating.Create(videoId, userId, type);
            if (ratingResult.Status != EResultStatus.Success)
                return ratingResult;

            return await _ratingRepository.SetRatingAsync(ratingResult.Data!);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult> RemoveRatingAsync(Guid videoId, Id userId)
    {
        return await ExecuteAsync(async () =>
        {
            return await _ratingRepository.RemoveRatingAsync(videoId, userId);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<VideoRatingStats>> GetStatsAsync(Guid videoId, Id? userId = null)
    {
        return await ExecuteAsync(async () =>
        {
            return await _ratingRepository.GetStatsAsync(videoId, userId);
        });
    }
}
