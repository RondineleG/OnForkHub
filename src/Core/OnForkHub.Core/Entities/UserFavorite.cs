namespace OnForkHub.Core.Entities;

/// <summary>
/// Represents a video favorited by a user.
/// </summary>
public class UserFavorite : BaseEntity
{
    protected UserFavorite(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    private UserFavorite() { }

    public Guid VideoId { get; private set; }

    public Id UserId { get; private set; } = null!;

    /// <summary>
    /// Creates a new user favorite association.
    /// </summary>
    /// <returns></returns>
    public static RequestResult<UserFavorite> Create(Guid videoId, Id userId)
    {
        try
        {
            var favorite = new UserFavorite { VideoId = videoId, UserId = userId };

            favorite.ValidateEntityState();
            return RequestResult<UserFavorite>.Success(favorite);
        }
        catch (DomainException ex)
        {
            return RequestResult<UserFavorite>.WithError(ex.Message);
        }
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
