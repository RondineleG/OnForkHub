namespace OnForkHub.Persistence.Repositories;

/// <summary>
/// Entity Framework repository implementation for Video entity.
/// </summary>
public class VideoRepositoryEF(IEntityFrameworkDataContext context) : IVideoRepositoryEF
{
    private const string EntityName = nameof(Video);

    private readonly IEntityFrameworkDataContext _context = context;

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> CreateAsync(Video video)
    {
        ArgumentNullException.ThrowIfNull(video);
        try
        {
            _context.Videos.Add(video);
            await _context.SaveChangesAsync();
            return RequestResult<Video>.Success(video);
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
    public async Task<RequestResult<Video>> DeleteAsync(Id id)
    {
        try
        {
            var video = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(_context.Videos, v => v.Id == id.ToString());

            if (video == null)
            {
                return RequestResult<Video>.WithError($"{EntityName} not found with ID: {id}.");
            }

            _context.Videos.Remove(video);
            await _context.SaveChangesAsync();
            return RequestResult<Video>.Success(video);
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
    public async Task<RequestResult<IEnumerable<Video>>> GetAllAsync(int page, int size)
    {
        try
        {
            var videos = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context.Videos.Include(v => v.Categories).OrderByDescending(v => v.CreatedAt).Skip((page - 1) * size).Take(size)
            );

            return RequestResult<IEnumerable<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Video>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> GetByIdAsync(Id id)
    {
        try
        {
            var video = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                _context.Videos.Include(v => v.Categories),
                v => v.Id == id.ToString()
            );

            return video != null ? RequestResult<Video>.Success(video) : RequestResult<Video>.WithError($"{EntityName} not found with ID: {id}.");
        }
        catch (Exception ex)
        {
            return RequestResult<Video>.WithError($"Error retrieving {EntityName}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Video>>> GetByUserIdAsync(Id userId, int page, int size)
    {
        try
        {
            var userIdString = userId.ToString();
            var videos = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context
                    .Videos.Include(v => v.Categories)
                    .Where(v => v.UserId != null && v.UserId.ToString() == userIdString)
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
            );

            return RequestResult<IEnumerable<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Video>>.WithError($"Error retrieving {EntityName} list by user: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<IEnumerable<Video>>> GetByCategoryIdAsync(long categoryId, int page, int size)
    {
        try
        {
            var categoryIdString = categoryId.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var videos = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context
                    .Videos.Include(v => v.Categories)
                    .Where(v => v.Categories.Any(c => c.Id.Contains(categoryIdString)))
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
            );

            return RequestResult<IEnumerable<Video>>.Success(videos);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Video>>.WithError($"Error retrieving {EntityName} list by category: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<RequestResult<Video>> UpdateAsync(Video video)
    {
        ArgumentNullException.ThrowIfNull(video);
        try
        {
            _context.Entry(video).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RequestResult<Video>.Success(video);
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
