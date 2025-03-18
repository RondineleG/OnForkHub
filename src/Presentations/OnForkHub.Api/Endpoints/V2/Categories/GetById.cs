namespace OnForkHub.Api.Endpoints.V2.Categories;

public class GetById(ILogger<GetById> logger, IUseCase<long, Category> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V2 = 2;

    private static readonly string Route = $"{GetVersionedRoute(V2)}/{{id}}";

    private readonly ILogger<GetById> _logger = logger;

    private readonly IUseCase<long, Category> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V2);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([FromRoute] long id, CancellationToken cancellationToken) =>
                    {
                        return await HandleUseCase(_useCase, _logger, id);
                    }
                )
            )
            .WithName("GetCategoryByIdV2")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V2)
            .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(10)))
            .WithDescription("Returns a specific category by its ID")
            .WithSummary("Get category by ID")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V2}" })
            .Produces<RequestResult<Category>>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
