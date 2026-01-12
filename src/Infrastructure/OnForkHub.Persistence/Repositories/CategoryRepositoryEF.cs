namespace OnForkHub.Persistence.Repositories;

public class CategoryRepositoryEF(IEntityFrameworkDataContext context) : ICategoryRepositoryEF
{
    private const string EntityName = nameof(Category);

    private readonly IEntityFrameworkDataContext _context = context;

    public async Task<RequestResult<Category>> CreateAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RequestResult<Category>.Success(category);
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

    public async Task<RequestResult<Category>> DeleteAsync(long id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return RequestResult<Category>.WithError($"{EntityName} not found with ID: {id}.");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RequestResult<Category>.Success(category);
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

    public async Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size)
    {
        try
        {
            var categories = await EntityFrameworkQueryableExtensions.ToListAsync(
                _context.Categories.OrderBy(c => c.Id).Skip((page - 1) * size).Take(size)
            );

            return RequestResult<IEnumerable<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Category>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }

    public async Task<RequestResult<Category>> GetByIdAsync(long id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            return category != null
                ? RequestResult<Category>.Success(category)
                : RequestResult<Category>.WithError($"{EntityName} not found with ID: {id}.");
        }
        catch (Exception ex)
        {
            return RequestResult<Category>.WithError($"Error retrieving {EntityName}: {ex.Message}");
        }
    }

    public async Task<RequestResult<Category>> UpdateAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        try
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RequestResult<Category>.Success(category);
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
    public async Task<RequestResult<(IEnumerable<Category> Items, int TotalCount)>> SearchAsync(
        string? searchTerm,
        int sortBy,
        bool sortDescending,
        int page,
        int pageSize
    )
    {
        try
        {
            var query = _context.Categories.AsQueryable();

            // Apply search term filter using EF.Functions.Like for case-insensitive search
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var pattern = $"%{searchTerm}%";
                query = query.Where(c => EF.Functions.Like(c.Name.Value, pattern));
            }

            // Get total count before pagination
            var totalCount = await EntityFrameworkQueryableExtensions.CountAsync(query);

            // Apply sorting
            query = sortBy switch
            {
                1 => sortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                _ => sortDescending ? query.OrderByDescending(c => c.Name.Value) : query.OrderBy(c => c.Name.Value),
            };

            // Apply pagination
            var items = await EntityFrameworkQueryableExtensions.ToListAsync(query.Skip((page - 1) * pageSize).Take(pageSize));

            return RequestResult<(IEnumerable<Category> Items, int TotalCount)>.Success((items, totalCount));
        }
        catch (Exception ex)
        {
            return RequestResult<(IEnumerable<Category> Items, int TotalCount)>.WithError($"Error searching {EntityName}: {ex.Message}");
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetTotalCountAsync()
    {
        return await EntityFrameworkQueryableExtensions.CountAsync(_context.Categories);
    }
}
