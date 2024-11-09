namespace OnForkHub.Application.Services;

public class CategoryService : ICategoryService
{
    public Task<RequestResult<Category>> CreateAsync(Category category)
    {
        throw new NotImplementedException();
    }

    public Task<RequestResult> DeleteAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<RequestResult<IEnumerable<Category>>> GetAsync(int page, int size)
    {
        throw new NotImplementedException();
    }

    public Task<RequestResult<Category>> GetByIdAsync(long id)
    {
        throw new NotImplementedException();
    }

    public Task<RequestResult<Category>> UpdateAsync(Category category)
    {
        throw new NotImplementedException();
    }
}
