namespace OnForkHub.Application.Services;

/// <summary>
/// Service implementation for Notification operations.
/// </summary>
public class NotificationService(INotificationRepositoryEF notificationRepository) : BaseService, INotificationService
{
    private readonly INotificationRepositoryEF _notificationRepository = notificationRepository;

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> SendAsync(
        string title,
        string message,
        Id userId,
        ENotificationType type,
        string? referenceId = null
    )
    {
        return await ExecuteAsync(async () =>
        {
            var notificationResult = Notification.Create(title, message, userId, type, referenceId);

            if (!notificationResult.Status.Equals(EResultStatus.Success) || notificationResult.Data is null)
            {
                return notificationResult;
            }

            return await _notificationRepository.CreateAsync(notificationResult.Data);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Notification>>> SendToManyAsync(
        string title,
        string message,
        IEnumerable<Id> userIds,
        ENotificationType type,
        string? referenceId = null
    )
    {
        return await ExecuteAsync(async () =>
        {
            var notifications = new List<Notification>();
            var errors = new List<string>();

            foreach (var userId in userIds)
            {
                var notificationResult = Notification.Create(title, message, userId, type, referenceId);

                if (!notificationResult.Status.Equals(EResultStatus.Success) || notificationResult.Data is null)
                {
                    errors.Add($"Failed to create notification for user {userId}: {notificationResult.Message}");
                    continue;
                }

                var createResult = await _notificationRepository.CreateAsync(notificationResult.Data);

                if (createResult.Status.Equals(EResultStatus.Success) && createResult.Data is not null)
                {
                    notifications.Add(createResult.Data);
                }
                else
                {
                    errors.Add($"Failed to save notification for user {userId}: {createResult.Message}");
                }
            }

            if (notifications.Count == 0 && errors.Count > 0)
            {
                return RequestResult<IEnumerable<Notification>>.WithError(string.Join("; ", errors));
            }

            return RequestResult<IEnumerable<Notification>>.Success(notifications);
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<Notification>> GetByIdAsync(string id)
    {
        return ExecuteAsync(async () => await _notificationRepository.GetByIdAsync(id));
    }

    /// <inheritdoc/>
    public Task<RequestResult<IEnumerable<Notification>>> GetUserNotificationsAsync(Id userId, int page, int pageSize)
    {
        return ExecuteAsync(async () => await _notificationRepository.GetByUserIdAsync(userId, page, pageSize));
    }

    /// <inheritdoc/>
    public Task<RequestResult<IEnumerable<Notification>>> GetUnreadNotificationsAsync(Id userId)
    {
        return ExecuteAsync(async () => await _notificationRepository.GetUnreadByUserIdAsync(userId));
    }

    /// <inheritdoc/>
    public async Task<int> GetUnreadCountAsync(Id userId)
    {
        return await _notificationRepository.GetUnreadCountAsync(userId);
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> MarkAsReadAsync(string id)
    {
        return await ExecuteAsync(async () =>
        {
            var notificationResult = await _notificationRepository.GetByIdAsync(id);

            if (!notificationResult.Status.Equals(EResultStatus.Success) || notificationResult.Data is null)
            {
                return notificationResult;
            }

            var notification = notificationResult.Data;
            var markResult = notification.MarkAsRead();

            if (!markResult.Status.Equals(EResultStatus.Success))
            {
                return RequestResult<Notification>.WithError(markResult.Message ?? "Failed to mark notification as read");
            }

            return await _notificationRepository.UpdateAsync(notification);
        });
    }

    /// <inheritdoc/>
    public Task<RequestResult<int>> MarkAllAsReadAsync(Id userId)
    {
        return ExecuteAsync(async () => await _notificationRepository.MarkAllAsReadAsync(userId));
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> ArchiveAsync(string id)
    {
        return await ExecuteAsync(async () =>
        {
            var notificationResult = await _notificationRepository.GetByIdAsync(id);

            if (!notificationResult.Status.Equals(EResultStatus.Success) || notificationResult.Data is null)
            {
                return notificationResult;
            }

            var notification = notificationResult.Data;
            var archiveResult = notification.Archive();

            if (!archiveResult.Status.Equals(EResultStatus.Success))
            {
                return RequestResult<Notification>.WithError(archiveResult.Message ?? "Failed to archive notification");
            }

            return await _notificationRepository.UpdateAsync(notification);
        });
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> DeleteAsync(string id)
    {
        return await ExecuteAsync(async () =>
        {
            var notificationResult = await _notificationRepository.GetByIdAsync(id);

            if (!notificationResult.Status.Equals(EResultStatus.Success) || notificationResult.Data is null)
            {
                return notificationResult;
            }

            var notification = notificationResult.Data;
            var deleteResult = notification.Delete();

            if (!deleteResult.Status.Equals(EResultStatus.Success))
            {
                return RequestResult<Notification>.WithError(deleteResult.Message ?? "Failed to delete notification");
            }

            return await _notificationRepository.UpdateAsync(notification);
        });
    }
}
