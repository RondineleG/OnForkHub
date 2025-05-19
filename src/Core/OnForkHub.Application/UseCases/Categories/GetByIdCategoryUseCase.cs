// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Application.UseCases.Categories;

public class GetByIdCategoryUseCase(ICategoryRepositoryEF categoryRepositoryEF) : IUseCase<long, Category>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;

    public async Task<RequestResult<Category>> ExecuteAsync(long request)
    {
        var result = await _categoryRepositoryEF.GetByIdAsync(request);
        return result?.Data is null ? RequestResult<Category>.WithNoContent() : RequestResult<Category>.Success(result.Data);
    }
}
