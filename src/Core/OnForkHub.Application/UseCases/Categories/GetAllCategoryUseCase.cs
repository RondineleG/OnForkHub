namespace OnForkHub.Application.UseCases.Categories;

public class GetAllCategoryUseCase(ICategoryRepositoryEF categoryRepositoryEF) : IUseCase<PaginationRequest, IEnumerable<Category>>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryRepositoryEF;

    public async Task<RequestResult<IEnumerable<Category>>> ExecuteAsync(PaginationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _categoryRepositoryEF.GetAllAsync(request.Page, request.ItemsPerPage);
        return result?.Data is null
            ? RequestResult<IEnumerable<Category>>.WithNoContent()
            : RequestResult<IEnumerable<Category>>.Success(result.Data);
    }
}
