namespace OnForkHub.Application.UseCases.Categories;

public class DeleteCategoryUseCase(ICategoryRepositoryEF categoryRepositoryEF) : IUseCase<string, Category>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;

    public async Task<RequestResult<Category>> ExecuteAsync(string request)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(request);

        if (!Guid.TryParse(request, out _))
        {
            return RequestResult<Category>.WithError("Invalid category ID format");
        }

        Id id = request;
        return await _categoryRepositoryEF.DeleteAsync(id);
    }
}
