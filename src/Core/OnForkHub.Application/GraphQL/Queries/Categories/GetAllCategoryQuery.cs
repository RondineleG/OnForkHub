using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.Application.GraphQL.Queries.Categories;

public class GetAllCategoryQuery : HotChocolateQueryBase
{
    public override string Name => "getAllCategories";
    public override string Description => "Returns all categories";

    public static async Task<RequestResult<IEnumerable<Category>>> HandleAsync(
        [Service] IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase
    )
    {
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        return await useCase.ExecuteAsync(request);
    }

    protected override void RegisterQuery(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field(Name)
            .ResolveWith<GetAllCategoryQuery>(q => HandleAsync(default!))
            .Type<ObjectType<RequestResult<IEnumerable<Category>>>>()
            .Description(Description);
    }
}
