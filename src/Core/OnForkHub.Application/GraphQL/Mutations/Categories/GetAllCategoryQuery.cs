namespace OnForkHub.Application.GraphQL.Mutations.Categories;

public class GetAllCategoryQuery
{
    public string Description { get; set; } = "Returns all categories";

    public string Name { get; set; } = "getAllCategories";

    public static async Task<RequestResult<IEnumerable<Category>>> HandleAsync(IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase)
    {
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        return await useCase.ExecuteAsync(request).ConfigureAwait(false);
    }
}
