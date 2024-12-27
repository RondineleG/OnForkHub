namespace OnForkHub.Application.UseCases.Categories;

public class GetAllCategoriesUseCase(ICategoryRepositoryEF categoryService) : IUseCase<PaginationRequestDto, IEnumerable<Category>>
{
    private readonly ICategoryRepositoryEF _categoryRepositoryEF = categoryService;

    public async Task<RequestResult<IEnumerable<Category>>> ExecuteAsync(PaginationRequestDto request)
    {
        var result = await _categoryRepositoryEF.GetAllAsync(request.Page, request.ItemsPerPage);
        return result?.Data is null
            ? RequestResult<IEnumerable<Category>>.WithNoContent()
            : RequestResult<IEnumerable<Category>>.Success(result.Data);
    }
}
