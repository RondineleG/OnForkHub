namespace OnForkHub.Api.Endpoints.Rest.V1.Categories;

using OnForkHub.Application.UseCases.Categories;
using OnForkHub.CrossCutting.Interfaces;

public class DeleteEndpoint(ILogger<DeleteEndpoint> logger, DeleteCategoryUseCase useCase) : BaseEndPoint<Category>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id:long}";

    private readonly ILogger<DeleteEndpoint> _logger = logger;

    private readonly DeleteCategoryUseCase _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapDelete(
                    Route,
                    async ([FromRoute] long id, CancellationToken cancellationToken) =>
                    {
                        return await HandleDeleteUseCase(_useCase, _logger, id);
                    }
                )
            )
            .WithName("DeleteCategoryV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Deletes a category by ID")
            .WithSummary("Delete category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
