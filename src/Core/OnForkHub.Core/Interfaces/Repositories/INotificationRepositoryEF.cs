namespace OnForkHub.Core.Interfaces.Repositories;

/// <summary>
/// Repository interface for Notification entity using Entity Framework.
/// </summary>
public interface INotificationRepositoryEF
{
    /// <summary>
    /// Creates a new notification.
    /// </summary>
    /// <param name="notification">The notification to create.</param>
    /// <returns>The created notification.</returns>
    Task<RequestResult<Notification>> CreateAsync(Notification notification);

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
    Task<RequestResult<IEnumerable<Notification>>> GetByUserIdAsync(Id userId, int page, int pageSize);

    /// <summary>
    /// Gets unread notifications for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>A list of unread notifications.</returns>
    Task<RequestResult<IEnumerable<Notification>>> GetUnreadByUserIdAsync(Id userId);

    /// <summary>
    /// Gets notifications by type for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="type">The notification type.</param>
    /// <param name="page">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A list of notifications.</returns>
    Task<RequestResult<IEnumerable<Notification>>> GetByTypeAsync(Id userId, ENotificationType type, int page, int pageSize);

    /// <summary>
    /// Updates a notification.
    /// </summary>
    /// <param name="notification">The notification to update.</param>
    /// <returns>The updated notification.</returns>
    Task<RequestResult<Notification>> UpdateAsync(Notification notification);

    /// <summary>
    /// Deletes a notification by ID.
    /// </summary>
    /// <param name="id">The notification ID.</param>
    /// <returns>The deleted notification.</returns>
    Task<RequestResult<Notification>> DeleteAsync(string id);

    /// <summary>
    /// Gets the count of unread notifications for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The count of unread notifications.</returns>
    Task<int> GetUnreadCountAsync(Id userId);

    /// <summary>
    /// Marks all notifications as read for a user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <returns>The number of notifications marked as read.</returns>
    Task<RequestResult<int>> MarkAllAsReadAsync(Id userId);
}
