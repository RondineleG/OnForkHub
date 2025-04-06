using HotChocolate.Types;

using OnForkHub.Core.GraphQL;

namespace OnForkHub.Application.GraphQL.Queries.Categories;

public class GetAllCategoryQuery : QueryGraphQLBase
{
    public static string Test()
    {
        return "GraphQL OK!";
    }

    public override void Register(IObjectTypeDescriptor descriptor)
    {
        descriptor.Field("getAllCategories").Resolve(ctx => Test()).Type<StringType>();
    }
}
