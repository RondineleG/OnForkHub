namespace OnForkHub.Persistence.Repositories;

public class CategoryRepositoryRavenDB(IDocumentStore documentStore) : ICategoryRepositoryRavenDB
{
    private const string EntityName = nameof(Category);
    private readonly IDocumentStore _documentStore = documentStore;

    public async Task<RequestResult<Category>> CreateAsync(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);
        try
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(category);
                await session.SaveChangesAsync();
                return RequestResult<Category>.Success(category);
            }
        }
        catch (Exception ex)
        {
            return RequestResult<Category>.WithError($"Error creating {EntityName}: {ex.Message}");
        }
    }

    public async Task<RequestResult<Category>> DeleteAsync(string id)
    {
        try
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                var category = await session.LoadAsync<Category>(id);
                if (category == null)
                {
                    return RequestResult<Category>.WithError($"{EntityName} not found with ID: {id}.");
                }

                session.Delete(category);
                await session.SaveChangesAsync();
                return RequestResult<Category>.Success(category);
            }
        }
        catch (Exception ex)
        {
            return RequestResult<Category>.WithError($"Error deleting {EntityName}: {ex.Message}");
        }
    }

    public async Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size)
    {
        try
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                var categories = await LinqExtensions.ToListAsync(session.Query<Category>().Skip((page - 1) * size).Take(size));

                return RequestResult<IEnumerable<Category>>.Success(categories);
            }
        }
        catch (Exception ex)
        {
            return RequestResult<IEnumerable<Category>>.WithError($"Error retrieving {EntityName} list: {ex.Message}");
        }
    }

    public async Task<RequestResult<Category>> GetByIdAsync(string id)
    {
        try
        {
            using (var session = _documentStore.OpenAsyncSession())
            {
                var category = await session.LoadAsync<Category>(id);
                return category != null
                    ? RequestResult<Category>.Success(category)
                    : RequestResult<Category>.WithError($"{EntityName} not found with ID: {id}.");
            }
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
            using (var session = _documentStore.OpenAsyncSession())
            {
                await session.StoreAsync(category);
                await session.SaveChangesAsync();
                return RequestResult<Category>.Success(category);
            }
        }
        catch (Exception ex)
        {
            return RequestResult<Category>.WithError($"Error updating {EntityName}: {ex.Message}");
        }
    }
}
