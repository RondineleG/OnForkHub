using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Interfaces.Repositories;

public interface ICategoryRepository
{
    Task<RequestResult<Category>> CreateAsync(Category category);

    Task<RequestResult<Category>> UpdateAsync(Category category);

    Task<RequestResult> DeleteAsync(long id);

    Task<RequestResult<Category>> GetByIdAsync(long id);

    Task<RequestResult<IEnumerable<Category>>> GetAsync(int page, int size);
}
