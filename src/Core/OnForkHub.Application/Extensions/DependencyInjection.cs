using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Application.GraphQL.Handlers;
using OnForkHub.Application.Services;
using OnForkHub.Core.Interfaces.GraphQL;
using OnForkHub.Core.Validations.Categories;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;
using OnForkHub.Persistence.Repositories;

namespace OnForkHub.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();
        services.AddScoped<ICategoryRepositoryEF, CategoryRepositoryEF>();
        services.AddScoped<ICategoryRepositoryRavenDB, CategoryRepositoryRavenDB>();
        services.AddScoped<IValidationService<Category>, CategoryValidationService>();

        return services;
    }

    public static IServiceCollection AddGraphQLAdapters(this IServiceCollection services)
    {
        services.AddScoped<IGraphQLQueryHandler<PaginationRequestDto, IEnumerable<Category>>, GetAllCategoriesHandler>();
        services.AddScoped<IGraphQLMutationHandler<CategoryRequestDto, Category>, CreateCategoryHandler>();

        services.AddScoped<IGraphQLQuery>(sp =>
        {
            var handler = sp.GetRequiredService<IGraphQLQueryHandler<PaginationRequestDto, IEnumerable<Category>>>();
            return new HotChocolateQueryAdapter<PaginationRequestDto, IEnumerable<Category>>(
                handler,
                "getAllCategories",
                "Retrieve all categories with pagination."
            );
        });

        services.AddScoped<IGraphQLMutation>(sp =>
        {
            var handler = sp.GetRequiredService<IGraphQLMutationHandler<CategoryRequestDto, Category>>();
            return new HotChocolateMutationAdapter<CategoryRequestDto, Category>(handler, "createCategory", "Create a new category.");
        });

        services.AddScoped<IGraphQLQuery>(sp =>
        {
            var handler = sp.GetRequiredService<IGraphQLQueryHandler<PaginationRequestDto, IEnumerable<Category>>>();
            return new GraphQLNetQueryAdapter<PaginationRequestDto, IEnumerable<Category>>(
                handler,
                "getAllCategories",
                "Retrieve all categories with pagination."
            );
        });

        services.AddScoped<IGraphQLMutation>(sp =>
        {
            var handler = sp.GetRequiredService<IGraphQLMutationHandler<CategoryRequestDto, Category>>();
            return new GraphQLNetMutationAdapter<CategoryRequestDto, Category>(handler, "createCategory", "Create a new category.");
        });

        return services;
    }
}
