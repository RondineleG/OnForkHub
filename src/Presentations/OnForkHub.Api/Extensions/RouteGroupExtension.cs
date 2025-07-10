using OnForkHub.Core.Interfaces.GraphQL;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.Api.Extensions;

public static class RouteGroupExtension
{
    public static RouteGroupBuilder MapGraphQLNetEndpoints(this RouteGroupBuilder group, GraphQLEndpointManager endpointManager)
    {
        foreach (var endpoint in endpointManager.Endpoints.OfType<GraphQLNetEndpoint>())
        {
            group.MapGraphQLNetGraphQL(endpoint.Path);
        }
        return group;
    }

    public static RouteGroupBuilder MapHotChocolateEndpoints(this RouteGroupBuilder group, GraphQLEndpointManager endpointManager)
    {
        foreach (var endpoint in endpointManager.Endpoints.OfType<HotChocolateEndpoint>())
        {
            group.MapHotChocolateGraphQL(endpoint.Path);
        }
        return group;
    }

    public static RouteGroupBuilder MapRestEndpoints(this RouteGroupBuilder group)
    {
        group
            .MapPost(
                "/categories",
                async (CategoryRequestDto request, [FromServices] IUseCase<CategoryRequestDto, Category> useCase) =>
                {
                    var result = await useCase.ExecuteAsync(request);
                    return Results.Created("/categories", result);
                }
            )
            .WithName("CreateCategory")
            .WithSummary("Create a new category")
            .WithDescription("This endpoint creates a new category and returns its details.");
        return group;
    }

    private static IEndpointRouteBuilder MapGraphQLNetGraphQL(this IEndpointRouteBuilder builder, string path)
    {
        builder.MapGraphQL(path);
        return builder;
    }

    private static IEndpointRouteBuilder MapHotChocolateGraphQL(this IEndpointRouteBuilder builder, string path)
    {
        builder.MapGraphQL(path: path);
        return builder;
    }
}
