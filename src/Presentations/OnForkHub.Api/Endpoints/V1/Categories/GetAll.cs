namespace OnForkHub.Api.Endpoints.V1.Categories;

public class GetAll(ILogger<GetAll> logger, IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<GetAll> _logger = logger;

    private readonly IUseCase<PaginationRequestDto, IEnumerable<Category>> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([AsParameters] PaginationRequestDto request, CancellationToken cancellationToken = default) =>
                    {
                        return await HandleUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("GetAllCategories")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(10)))
            .WithDescription("Returns all categories")
            .WithSummary("List categories")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<IEnumerable<Category>>>();

        return Task.FromResult(RequestResult.Success());
    }
}
