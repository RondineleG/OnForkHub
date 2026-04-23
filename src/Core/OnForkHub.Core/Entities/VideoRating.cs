namespace OnForkHub.Core.Entities;

using OnForkHub.Core.Enums;

/// <summary>
/// Represents a user's rating (Like/Dislike) on a video.
/// </summary>
public class VideoRating : BaseEntity
{
    protected VideoRating(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    private VideoRating() { }

    public Guid VideoId { get; private set; }

    public Id UserId { get; private set; } = null!;

    public ERatingType Type { get; private set; }

    /// <summary>
    /// Creates a new video rating.
    /// </summary>
    /// <returns></returns>
    public static RequestResult<VideoRating> Create(Guid videoId, Id userId, ERatingType type)
    {
        try
        {
            var rating = new VideoRating
            {
                VideoId = videoId,
                UserId = userId,
                Type = type,
            };

            rating.ValidateEntityState();
            return RequestResult<VideoRating>.Success(rating);
        }
        catch (DomainException ex)
        {
            return RequestResult<VideoRating>.WithError(ex.Message);
        }
    }

    /// <summary>
    /// Updates the rating type.
    /// </summary>
    public void UpdateType(ERatingType type)
    {
        Type = type;
        Update();
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();

        var result = ValidationResult.Success();

        result.AddErrorIf(() => UserId == null, "UserId is required.", nameof(UserId));
        result.AddErrorIf(() => VideoId == Guid.Empty, "VideoId is required.", nameof(VideoId));

        if (result.HasError)
        {
            throw new DomainException(result.ErrorMessage);
        }
    }
}
