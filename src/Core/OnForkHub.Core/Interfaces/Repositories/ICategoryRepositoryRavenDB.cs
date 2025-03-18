namespace OnForkHub.Core.Interfaces.Repositories;

public interface ICategoryRepositoryRavenDB
{
    Task<RequestResult<Category>> CreateAsync(Category category);

    Task<RequestResult<Category>> DeleteAsync(string id);

    Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size);

    Task<RequestResult<Category>> GetByIdAsync(string id);

    Task<RequestResult<Category>> UpdateAsync(Category category);
}
