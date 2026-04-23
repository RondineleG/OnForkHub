namespace OnForkHub.Api.Endpoints.Rest.V1.Categories;

using OnForkHub.CrossCutting.Interfaces;

public class GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IUseCase<long, Category> useCase) : BaseEndPoint<Category>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id:long}";

    private readonly ILogger<GetByIdEndpoint> _logger = logger;

    private readonly IUseCase<long, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([FromRoute] long id, CancellationToken cancellationToken) =>
                    {
                        return await HandleUseCase(_useCase, _logger, id, cancellationToken);
                    }
                )
            )
            .WithName("GetCategoryByIdV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Gets a category by ID")
            .WithSummary("Get category by ID")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Category>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        return Task.FromResult(RequestResult.Success());
    }
}
