namespace OnForkHub.Application.GraphQL.Queries.Categories;

public class GetAllCategoryQuery : QueryGraphQLBase
{
    public static async Task<RequestResult<IEnumerable<Category>>> HandleAsync(
        [Service] IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase
    )
    {
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        return await useCase.ExecuteAsync(request);
    }

    public override void Register(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field("getAllCategories")
            .ResolveWith<GetAllCategoryQuery>(q => HandleAsync(default!))
            .Type<ObjectType<RequestResult<IEnumerable<Category>>>>()
            .Description("Returns all categories");
    }
}
