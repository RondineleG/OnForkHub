namespace OnForkHub.Application.UseCases.Categories;

public class GetCategoryByIdUseCase(ICategoryRepositoryEF categoryRepositoryEF) : IUseCase<long, Category>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;

    public async Task<RequestResult<Category>> ExecuteAsync(long id)
    {
        var result = await _categoryRepositoryEF.GetByIdAsync(id);
        return result?.Data is null ? RequestResult<Category>.WithNoContent() : RequestResult<Category>.Success(result.Data);
    }
}