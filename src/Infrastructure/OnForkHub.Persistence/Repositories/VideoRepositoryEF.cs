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
                _context
                    .Videos.AsNoTracking()
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Include(v => v.Categories)
                    .AsSplitQuery()
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
                _context.Videos.AsNoTracking().Include(v => v.Categories).AsSplitQuery(),
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
                    .Videos.AsNoTracking()
                    .Where(v => v.UserId != null && v.UserId.ToString() == userIdString)
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Include(v => v.Categories)
                    .AsSplitQuery()
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
                    .Videos.AsNoTracking()
                    .Where(v => v.Categories.Any(c => c.Id.Contains(categoryIdString)))
                    .OrderByDescending(v => v.CreatedAt)
                    .Skip((page - 1) * size)
                    .Take(size)
                    .Include(v => v.Categories)
                    .AsSplitQuery()
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

    /// <inheritdoc/>
    public async Task<RequestResult<(IEnumerable<Video> Items, int TotalCount)>> SearchAsync(
        string? searchTerm,
        long? categoryId,
        string? userId,
        DateTime? fromDate,
        DateTime? toDate,
        int sortBy,
        bool sortDescending,
        int page,
        int pageSize
    )
    {
        try
        {
            // Start with base query without Include for filtering (optimized for count)
            var baseQuery = _context.Videos.AsNoTracking().AsQueryable();

            // Apply search term filter using EF.Functions.Like for case-insensitive search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var pattern = $"%{searchTerm}%";
                baseQuery = baseQuery.Where(v => EF.Functions.Like(v.Title.Value, pattern) || EF.Functions.Like(v.Description, pattern));
            }

            // Apply category filter
            if (categoryId.HasValue)
            {
                var categoryIdString = categoryId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                baseQuery = baseQuery.Where(v => v.Categories.Any(c => c.Id.Contains(categoryIdString)));
            }

            // Apply user filter
            if (!string.IsNullOrWhiteSpace(userId))
            {
                baseQuery = baseQuery.Where(v => v.UserId != null && v.UserId.ToString() == userId);
            }

            // Apply date range filters
            if (fromDate.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.CreatedAt >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                baseQuery = baseQuery.Where(v => v.CreatedAt <= toDate.Value);
            }

            // Get total count without Include (optimized count query)
            var totalCount = await EntityFrameworkQueryableExtensions.CountAsync(baseQuery);

            // Apply sorting
            var sortedQuery = sortBy switch
            {
                1 => sortDescending ? baseQuery.OrderByDescending(v => v.Title.Value) : baseQuery.OrderBy(v => v.Title.Value),
                2 => sortDescending ? baseQuery.OrderByDescending(v => v.UpdatedAt) : baseQuery.OrderBy(v => v.UpdatedAt),
                _ => sortDescending ? baseQuery.OrderByDescending(v => v.CreatedAt) : baseQuery.OrderBy(v => v.CreatedAt),
            };

            // Apply pagination and Include categories only for final result (split query for performance)
            var items = await EntityFrameworkQueryableExtensions.ToListAsync(
                sortedQuery.Skip((page - 1) * pageSize).Take(pageSize).Include(v => v.Categories).AsSplitQuery()
            );

            return RequestResult<(IEnumerable<Video> Items, int TotalCount)>.Success((items, totalCount));
        }
        catch (Exception ex)
        {
            return RequestResult<(IEnumerable<Video> Items, int TotalCount)>.WithError($"Error searching {EntityName}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync()
    {
        return await EntityFrameworkQueryableExtensions.CountAsync(_context.Videos);
    }
}
