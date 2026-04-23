namespace OnForkHub.Application.UseCases.Categories;

public class GetAllCategoryUseCase(ICategoryServiceRavenDB categoryServiceRavenDB) : IUseCase<PaginationRequestDto, IEnumerable<Category>>
{
    private readonly ICategoryServiceRavenDB _categoryServiceRavenDB = categoryServiceRavenDB;

    public async Task<RequestResult<IEnumerable<Category>>> ExecuteAsync(PaginationRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await _categoryServiceRavenDB.GetAllAsync(request.Page, request.ItemsPerPage);
        return result?.Data is null
            ? RequestResult<IEnumerable<Category>>.WithNoContent()
            : RequestResult<IEnumerable<Category>>.Success(result.Data);
    }
}
