using GraphQL.Types;

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.CrossCutting.GraphQL.GraphQLNet;

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
