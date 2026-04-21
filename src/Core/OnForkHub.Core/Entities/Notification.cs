namespace OnForkHub.Core.Entities;

public class Notification : BaseEntity
{
    public Notification(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    protected Notification() { }

    public Id UserId { get; private set; } = null!;

    public Title Title { get; private set; } = null!;

    public string Message { get; private set; } = string.Empty;

    public ENotificationType Type { get; private set; }

    public ENotificationStatus Status { get; private set; }

    public string? ReferenceId { get; private set; }

    public DateTime? ReadAt { get; private set; }

    public static RequestResult<Notification> Create(string title, string message, Id userId, ENotificationType type, string? referenceId = null)
    {
        try
        {
            var notification = new Notification
            {
                Title = Title.Create(title),
                Message = message ?? throw new ArgumentNullException(nameof(message)),
                UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
                Type = type,
                Status = ENotificationStatus.Unread,
                ReferenceId = referenceId,
            };

            notification.ValidateEntityState();
            return RequestResult<Notification>.Success(notification);
        }
        catch (DomainException ex)
        {
            return RequestResult<Notification>.WithError(ex.Message);
        }
    }

    public static RequestResult<Notification> Load(
        Id id,
        string title,
        string message,
        Id userId,
        ENotificationType type,
        ENotificationStatus status,
        string? referenceId,
        DateTime? readAt,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        try
        {
            var notification = new Notification(id, createdAt, updatedAt)
            {
                Title = Title.Create(title),
                Message = message ?? throw new ArgumentNullException(nameof(message)),
                UserId = userId ?? throw new ArgumentNullException(nameof(userId)),
                Type = type,
                Status = status,
                ReferenceId = referenceId,
                ReadAt = readAt,
            };

            notification.ValidateEntityState();
            return RequestResult<Notification>.Success(notification);
        }
        catch (DomainException ex)
        {
            return RequestResult<Notification>.WithError(ex.Message);
        }
    }

    public RequestResult MarkAsRead()
    {
        try
        {
            if (Status == ENotificationStatus.Read)
            {
                return RequestResult.Success();
            }

            Status = ENotificationStatus.Read;
            ReadAt = DateTime.UtcNow;
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    public RequestResult MarkAsUnread()
    {
        try
        {
            if (Status == ENotificationStatus.Unread)
            {
                return RequestResult.Success();
            }

            Status = ENotificationStatus.Unread;
            ReadAt = null;
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    public RequestResult Archive()
    {
        try
        {
            if (Status == ENotificationStatus.Archived)
            {
                return RequestResult.Success();
            }

            Status = ENotificationStatus.Archived;
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    public RequestResult Delete()
    {
        try
        {
            Status = ENotificationStatus.Deleted;
            Update();
            return RequestResult.Success();
        }
        catch (DomainException ex)
        {
            return RequestResult.WithError(ex.Message);
        }
    }

    protected override string GetCollectionName()
    {
        return "notifications";
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();

        var validationResult = ValidationResult.Success();

        validationResult
            .AddErrorIf(() => UserId == null, "UserId is required", nameof(UserId))
            .AddErrorIf(() => string.IsNullOrWhiteSpace(Message), "Message is required", nameof(Message))
            .AddErrorIf(() => Message.Length > 1000, "Message cannot exceed 1000 characters", nameof(Message));

        if (Title != null)
        {
            validationResult.Merge(Title.Validate());
        }
        else
        {
            validationResult.AddError("Title is required", nameof(Title));
        }

        if (ReadAt.HasValue && Status != ENotificationStatus.Read)
        {
            validationResult.AddError("ReadAt should only be set when status is Read", nameof(ReadAt));
        }

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }
}
