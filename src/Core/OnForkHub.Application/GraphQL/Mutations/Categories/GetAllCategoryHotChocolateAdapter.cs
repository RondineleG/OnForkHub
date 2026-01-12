using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.Application.GraphQL.Mutations.Categories;

public class GetAllCategoryHotChocolateAdapter : HotChocolateQueryBase
{
    private readonly GetAllCategoryQuery _query = new();

    public override string Description => _query.Description;

    public override string Name => _query.Name;

    protected override void RegisterQuery(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field(Name)
            .ResolveWith<GetAllCategoryQuery>(q => GetAllCategoryQuery.HandleAsync(default!))
            .Type<ObjectType<RequestResult<IEnumerable<Category>>>>()
            .Description(Description);
    }
}
