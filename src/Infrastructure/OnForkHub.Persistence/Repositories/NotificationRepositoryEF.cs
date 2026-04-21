namespace OnForkHub.Persistence.Repositories;

/// <summary>
/// Entity Framework repository implementation for Notification entity.
/// </summary>
public class NotificationRepositoryEF(IEntityFrameworkDataContext context) : INotificationRepositoryEF
{
    private const string EntityName = nameof(Notification);

    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> CreateAsync(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        try
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            return RequestResult<Notification>.Success(notification);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "create", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("create", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> GetByIdAsync(string id)
    {
        try
        {
            var notification = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Notifications.AsNoTracking(), n => n.Id == id);

            return notification != null
                ? RequestResult<Notification>.Success(notification)
                : RequestResult<Notification>.WithError($"{EntityName} not found with ID: {id}.");
        }
        catch (Exception ex)
        {
            return RequestResult<Notification>.WithError($"Error retrieving {EntityName}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Notification>>> GetByUserIdAsync(Id userId, int page, int pageSize)
    {
        try
        {
            var userIdValue = userId.Value;
            var notifications = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context
                    .Notifications.AsNoTracking()
                    .Where(n => n.UserId.Value == userIdValue && n.Status != ENotificationStatus.Deleted)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
            );

            return RequestResult<IEnumerable<Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Notification>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Notification>>> GetUnreadByUserIdAsync(Id userId)
    {
        try
        {
            var userIdValue = userId.Value;
            var notifications = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context
                    .Notifications.AsNoTracking()
                    .Where(n => n.UserId.Value == userIdValue && n.Status == ENotificationStatus.Unread)
                    .OrderByDescending(n => n.CreatedAt)
            );

            return RequestResult<IEnumerable<Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Notification>>.WithError($"Error retrieving unread {EntityName} list: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Notification>>> GetByTypeAsync(Id userId, ENotificationType type, int page, int pageSize)
    {
        try
        {
            var userIdValue = userId.Value;
            var notifications = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context
                    .Notifications.AsNoTracking()
                    .Where(n => n.UserId.Value == userIdValue && n.Type == type && n.Status != ENotificationStatus.Deleted)
                    .OrderByDescending(n => n.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
            );

            return RequestResult<IEnumerable<Notification>>.Success(notifications);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Notification>>.WithError($"Error retrieving {EntityName} list by type: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> UpdateAsync(Notification notification)
    {
        ArgumentNullException.ThrowIfNull(notification);
        try
        {
            _context.Entry(notification).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RequestResult<Notification>.Success(notification);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "update", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("update", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Notification>> DeleteAsync(string id)
    {
        try
        {
            var notification = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Notifications, n => n.Id == id);

            if (notification == null)
            {
                return RequestResult<Notification>.WithError($"{EntityName} not found with ID: {id}.");
            }

            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
            return RequestResult<Notification>.Success(notification);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "delete", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("delete", ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetUnreadCountAsync(Id userId)
    {
        var userIdValue = userId.Value;
        return await EntityFrameworkQueryableExtensions.CountAsync(
            _context.Notifications.AsNoTracking().Where(n => n.UserId.Value == userIdValue && n.Status == ENotificationStatus.Unread)
        );
    }

    /// <inheritdoc/>
    public async Task<RequestResult<int>> MarkAllAsReadAsync(Id userId)
    {
        try
        {
            var userIdValue = userId.Value;
            var unreadNotifications = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context.Notifications.Where(n => n.UserId.Value == userIdValue && n.Status == ENotificationStatus.Unread)
            );

            foreach (var notification in unreadNotifications)
            {
                notification.MarkAsRead();
                _context.Entry(notification).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return RequestResult<int>.Success(unreadNotifications.Count);
        }
        catch (DbUpdateException ex)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "update", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("update", ex.Message);
        }
    }
}
