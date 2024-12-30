namespace OnForkHub.Persistence.Repositories;

public class CategoryRepositoryCosmos(ICosmosContainerContext<Category> context) : IBaseRepository<Category>, ICategoryRepositoryComos
{
    private const string EntityName = nameof(Category);
    private readonly ICosmosContainerContext<Category> _context = context;

    public async Task<RequestResult<Category>> CreateAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        try
        {
            var result = await _context.CreateAsync(category);
            return RequestResult<Category>.Success(result);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            var persistenceException = PersistenceExceptionHandler.HandleDbException(ex, "create", EntityName);
            throw persistenceException;
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("create", ex.Message);
        }
    }

    public async Task<RequestResult<Category>> UpdateAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        try
        {
            var result = await _context.UpdateAsync(category);
            return RequestResult<Category>.Success(result);
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return RequestResult<Category>.WithError($"{EntityName} not found with ID: {category.Id}");
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("update", ex.Message);
        }
    }

    public async Task<RequestResult<Category>> DeleteAsync(Id id)
    {
        try
        {
            var deleted = await _context.DeleteAsync(id, id);
            return (RequestResult<Category>)(
                deleted ? RequestResult.Success() : RequestResult<Category>.WithError($"{EntityName} not found with ID: {id}.")
            );
        }
        catch (Exception ex) when (ex is not PersistenceException)
        {
            throw new DatabaseOperationException("delete", ex.Message);
        }
    }

    public async Task<RequestResult<Category>> GetByIdAsync(Id id)
    {
        try
        {
            var category = await _context.GetByIdAsync(id, id);
            return category != null
                ? RequestResult<Category>.Success(category)
                : RequestResult<Category>.WithError($"{EntityName} not found with ID: {id}.");
        }
        catch (Exception ex)
        {
            return RequestResult<Category>.WithError($"Error retrieving {EntityName}: {ex.Message}");
        }
    }

    public async Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size)
    {
        try
        {
            var categories = await _context.GetPagedAsync(page, size);
            return RequestResult<IEnumerable<Category>>.Success(categories);
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Category>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }
}