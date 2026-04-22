namespace OnForkHub.Core.Entities;

/// <summary>
/// Represents a record of a user viewing a video.
/// </summary>
public class ViewHistory : BaseEntity
{
    protected ViewHistory(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    private ViewHistory() { }

    public Guid VideoId { get; private set; }

    public Id UserId { get; private set; } = null!;

    public DateTime LastViewedAt { get; private set; }

    /// <summary>
    /// Creates a new view history record.
    /// </summary>
    /// <returns></returns>
    public static RequestResult<ViewHistory> Create(Guid videoId, Id userId)
    {
        try
        {
            var history = new ViewHistory
            {
                VideoId = videoId,
                UserId = userId,
                LastViewedAt = DateTime.UtcNow,
            };

            history.ValidateEntityState();
            return RequestResult<ViewHistory>.Success(history);
        }
        catch (DomainException ex)
        {
            return RequestResult<ViewHistory>.WithError(ex.Message);
        }
    }

    /// <summary>
    /// Updates the last viewed timestamp.
    /// </summary>
    public void UpdateLastViewed()
    {
        LastViewedAt = DateTime.UtcNow;
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
