using HotChocolate.Types;

using OnForkHub.Core.GraphQL;

namespace OnForkHub.Application.GraphQL.Queries.Categories;

public class GetAllCategoryQuery : QueryGraphQLBase
{
    public override void Register(IObjectTypeDescriptor descriptor)
    {
        descriptor.Field("getAllCategories").Resolve(ctx => Test()).Type<StringType>();
    }
}
