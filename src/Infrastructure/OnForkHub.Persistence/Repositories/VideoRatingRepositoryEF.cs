namespace OnForkHub.Persistence.Repositories;

using Microsoft.EntityFrameworkCore;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Requests;
using OnForkHub.Persistence.Contexts.Base;

/// <summary>
/// EF implementation for video rating repository.
/// </summary>
public sealed class VideoRatingRepositoryEF(IEntityFrameworkDataContext context) : IVideoRatingRepository
{
    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<VideoRating>> SetRatingAsync(VideoRating rating)
    {
        try
        {
            var existing = await _context.VideoRatings.FirstOrDefaultAsync(x => x.VideoId == rating.VideoId && x.UserId == rating.UserId);

            if (existing != null)
            {
                existing.UpdateType(rating.Type);
                _context.VideoRatings.Update(existing);
            }
            else
            {
                _context.VideoRatings.Add(rating);
            }

            await _context.SaveChangesAsync();
            return RequestResult<VideoRating>.Success(existing ?? rating);
        }
        catch (Exception ex)
        {
            return RequestResult<VideoRating>.WithError($"Error setting rating: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult> RemoveRatingAsync(Guid videoId, Id userId)
    {
        try
        {
            var rating = await _context.VideoRatings.FirstOrDefaultAsync(x => x.VideoId == videoId && x.UserId == userId);

            if (rating != null)
            {
                _context.VideoRatings.Remove(rating);
                await _context.SaveChangesAsync();
            }

            return RequestResult.Success();
        }
        catch (Exception ex)
        {
            return RequestResult.WithError($"Error removing rating: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<VideoRatingStats>> GetStatsAsync(Guid videoId, Id? userId = null)
    {
        try
        {
            var ratings = await _context.VideoRatings.Where(x => x.VideoId == videoId).ToListAsync();

            var likes = ratings.Count(x => x.Type == ERatingType.Like);
            var dislikes = ratings.Count(x => x.Type == ERatingType.Dislike);

            ERatingType? userRating = null;
            if (userId != null)
            {
                userRating = ratings.FirstOrDefault(x => x.UserId == userId)?.Type;
            }

            return RequestResult<VideoRatingStats>.Success(new VideoRatingStats(likes, dislikes, userRating));
        }
        catch (Exception ex)
        {
            return RequestResult<VideoRatingStats>.WithError($"Error fetching rating stats: {ex.Message}");
        }
    }
}
