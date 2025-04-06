namespace OnForkHub.Api.Endpoints.Rest.V2.Categories;

public class Delete(ILogger<Delete> logger, IUseCase<long, Category> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V2 = 2;

    private static readonly string Route = $"{GetVersionedRoute(V2)}/{{id}}";

    private readonly ILogger<Delete> _logger = logger;

    private readonly IUseCase<long, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V2);
        ConfigureEndpoint(
                app.MapDelete(
                    Route,
                    async ([FromRoute] long id, CancellationToken cancellationToken) =>
                    {
                        return await HandleDeleteUseCase(_useCase, _logger, id);
                    }
                )
            )
            .WithName("DeleteCategoryV2")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V2)
            .WithDescription("Deletes a category")
            .WithSummary("Delete category")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V2}" })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();
        return Task.FromResult(RequestResult.Success());
    }
}
