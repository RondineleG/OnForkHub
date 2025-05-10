using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Application.GraphQL.Handlers;
using OnForkHub.Core.GraphQL;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphQLAdapters(this IServiceCollection services)
    {
        // Handlers
        services.AddScoped<IGraphQLQueryHandler<PaginationRequestDto, IEnumerable<Category>>, GetAllCategoriesHandler>();
        services.AddScoped<IGraphQLMutationHandler<CategoryRequestDto, Category>, CreateCategoryHandler>();

        // HotChocolate Adapters
        services.AddScoped<HotChocolateQueryAdapter<PaginationRequestDto, IEnumerable<Category>>>();
        services.AddScoped<HotChocolateMutationAdapter<CategoryRequestDto, Category>>();

        // GraphQL.Net Adapters
        services.AddScoped<GraphQLNetQueryAdapter<PaginationRequestDto, IEnumerable<Category>>>();
        services.AddScoped<GraphQLNetMutationAdapter<CategoryRequestDto, Category>>();

        return services;
    }
}
