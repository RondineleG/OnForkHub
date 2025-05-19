// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Interfaces.Repositories;

public interface ICategoryRepositoryEF
{
    Task<RequestResult<Category>> CreateAsync(Category category);

    Task<RequestResult<Category>> DeleteAsync(long id);

    Task<RequestResult<IEnumerable<Category>>> GetAllAsync(int page, int size);

    Task<RequestResult<Category>> GetByIdAsync(long id);

    Task<RequestResult<Category>> UpdateAsync(Category category);
}
