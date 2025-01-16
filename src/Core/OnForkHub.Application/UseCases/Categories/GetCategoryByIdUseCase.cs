namespace OnForkHub.Application.UseCases.Categories;

public class GetCategoryByIdUseCase(ICategoryRepositoryEF categoryRepositoryEF) : IUseCase<long, Category>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;

    public async Task<RequestResult<Category>> ExecuteAsync(long request)
    {
        var result = await _categoryRepositoryEF.GetByIdAsync(request);
        return result?.Data is null ? RequestResult<Category>.WithNoContent() : RequestResult<Category>.Success(result.Data);
    }
}