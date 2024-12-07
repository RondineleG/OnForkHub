namespace OnForkHub.Core.Interfaces.Repositories;

public interface ICategoryRepositoryComos
{
    Task<RequestResult<Category>> CreateAsync(Category category);

    Task<RequestResult<Category>> UpdateAsync(Category category);

    Task<RequestResult<Category>> DeleteAsync(Id id);

    Task<RequestResult<Category>> GetByIdAsync(Id id);

    Task<RequestResult<IEnumerable<Category>>> GetAsync(int page, int size);
}