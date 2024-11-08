using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Interfaces.Services;

public interface ICategoryService
{
    Task<RequestResult<Category>> CreateAsync(Category category);

    Task<RequestResult<Category>> UpdateAsync(Category category);

    Task<RequestResult> DeleteAsync(long id);

    Task<RequestResult<Category>> GetByIdAsync(long id);

    Task<RequestResult<IEnumerable<Category>>> GetAsync(int page, int size);
}
