namespace OnForkHub.Core.Interfaces.Services;

/// <summary>
/// Service interface for Notification operations.
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a user.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="referenceId">Optional reference ID.</param>
    /// <returns>The created notification.</returns>
    Task<RequestResult<Notification>> SendAsync(string title, string message, Id userId, ENotificationType type, string? referenceId = null);

    /// <summary>
    /// Sends a notification to multiple users.
    /// </summary>
    /// <param name="title">The notification title.</param>
    /// <param name="message">The notification message.</param>
    /// <param name="userIds">The user IDs.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="referenceId">Optional reference ID.</param>
    /// <returns>The created notifications.</returns>
    Task<RequestResult<IEnumerable<Notification>>> SendToManyAsync(
        string title,
        string message,
        IEnumerable<Id> userIds,
        ENotificationType type,
        string? referenceId = null
    );

    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>The notification.</returns>
    Task<RequestResult<Notification>> GetByIdAsync(string id);

    /// <summary>
    /// Gets notifications for a user with pagination.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A list of notifications.</returns>
    Task<RequestResult<IEnumerable<Notification>>> GetUserNotificationsAsync(Id userId, int page, int pageSize);

    /// <summary>
    /// Gets unread notifications for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of unread notifications.</returns>
    Task<RequestResult<IEnumerable<Notification>>> GetUnreadNotificationsAsync(Id userId);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The count of unread notifications.</returns>
    Task<int> GetUnreadCountAsync(Id userId);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>The updated notification.</returns>
    Task<RequestResult<Notification>> MarkAsReadAsync(string id);

    /// <summary>
    /// Marks all notifications as read for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The number of notifications marked as read.</returns>
    Task<RequestResult<int>> MarkAllAsReadAsync(Id userId);

    /// <summary>
    /// Archives a notification.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>The updated notification.</returns>
    Task<RequestResult<Notification>> ArchiveAsync(string id);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>The deleted notification.</returns>
    Task<RequestResult<Notification>> DeleteAsync(string id);
}
