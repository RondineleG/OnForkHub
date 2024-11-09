namespace OnForkHub.Persistence.Repositories;

public class CategoryRepository(EntityFrameworkDataContext context) : ICategoryRepository
{
    private readonly EntityFrameworkDataContext _context = context;

    public async Task<RequestResult<Category>> CreateAsync(Category category)
    {
        try
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return RequestResult<Category>.Success(category);
        }
        catch (DbUpdateException exception)
        {
            return RequestResult<Category>.WithError(CustomMessageHandler.DbUpdateError(exception));
        }
        catch (Exception exception)
        {
            return RequestResult<Category>.WithError(CustomMessageHandler.UnexpectedError("create category", exception.Message));
        }
    }

    public async Task<RequestResult<Category>> UpdateAsync(Category category)
    {
        try
        {
            _context.Entry(category).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return RequestResult<Category>.Success(category);
        }
        catch (DbUpdateException exception)
        {
            return RequestResult<Category>.WithError(CustomMessageHandler.DbUpdateError(exception));
        }
        catch (Exception exception)
        {
            return RequestResult<Category>.WithError(CustomMessageHandler.UnexpectedError("update category", exception.Message));
        }
    }

    public async Task<RequestResult> DeleteAsync(long id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return RequestResult.WithError(CustomMessageHandler.EntityNotFound("Category", id));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RequestResult.Success();
        }
        catch (DbUpdateException exception)
        {
            return RequestResult.WithError(CustomMessageHandler.DbUpdateError(exception));
        }
        catch (Exception exception)
        {
            return RequestResult.WithError(CustomMessageHandler.UnexpectedError("delete category", exception.Message));
        }
    }

    public async Task<RequestResult<Category>> GetByIdAsync(long id)
    {
        try
        {
            var category = await _context.Categories.FindAsync(id);
            return category != null
                ? RequestResult<Category>.Success(category)
                : RequestResult<Category>.WithError(CustomMessageHandler.EntityNotFound("Category", id));
        }
        catch (Exception exception)
        {
            return RequestResult<Category>.WithError(CustomMessageHandler.UnexpectedError("get category", exception.Message));
        }
    }

    public async Task<RequestResult<IEnumerable<Category>>> GetAsync(int page, int size)
    {
        try
        {
            var categories = await _context.Categories.OrderBy(c => c.Id).Skip((page - 1) * size).Take(size).ToListAsync();

            return RequestResult<IEnumerable<Category>>.Success(categories);
        }
        catch (Exception exception)
        {
            return RequestResult<IEnumerable<Category>>.WithError(CustomMessageHandler.UnexpectedError("get categories", exception.Message));
        }
    }
}
