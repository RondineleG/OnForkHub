namespace OnForkHub.Application.UseCases.Categories;

public class DeleteCategoryUseCase(ICategoryRepositoryEF categoryRepositoryEF) : IUseCase<long, Category>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;

    public async Task<RequestResult<Category>> ExecuteAsync(long id)
    {
        var result = await _categoryRepositoryEF.DeleteAsync(id);
        return result.Status != EResultStatus.Success || result.Data is null
            ? RequestResult<Category>.WithError("Failed to delete category")
            : RequestResult<Category>.Success(result.Data);
    }
}