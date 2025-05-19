// The .NET Foundation licenses this file to you under the MIT license.

using GraphQL.Types;

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.Application.GraphQL.Mutations.Categories;

public class GetAllCategoryGraphQLNetAdapter : GraphQLNetQueryBase
{
    public override string Description => new GetAllCategoryQuery().Description;

    public override string Name => new GetAllCategoryQuery().Name;

    protected override void RegisterQuery(ObjectGraphType graphType)
    {
        graphType
            .Field<ListGraphType<CategoryGraphType>>(Name)
            .Description(Description)
            .ResolveAsync(async context =>
            {
                var serviceProvider = context.RequestServices ?? throw new InvalidOperationException("RequestServices is null.");

                var useCase = serviceProvider.GetRequiredService<IUseCase<PaginationRequestDto, IEnumerable<Category>>>();

                var result = await GetAllCategoryQuery.HandleAsync(useCase);
                return result.Data;
            });
    }
}

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

public class GetAllCategoryQuery
{
    public string Description { get; set; } = "Returns all categories";

    public string Name { get; set; } = "getAllCategories";

    public static async Task<RequestResult<IEnumerable<Category>>> HandleAsync(IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase)
    {
        var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
        return await useCase.ExecuteAsync(request);
    }
}

public class CategoryGraphType : ObjectGraphType<Category>
{
    public CategoryGraphType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
    }
}
