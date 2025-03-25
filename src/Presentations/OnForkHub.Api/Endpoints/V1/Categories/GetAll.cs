namespace OnForkHub.Api.Endpoints.V1.Categories;

public class GetAll(ILogger<GetAll> logger, IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V3 = 3;

    private static readonly string Route = GetVersionedRoute(V3);

    private readonly ILogger<GetAll> _logger = logger;

    private readonly IUseCase<PaginationRequestDto, IEnumerable<Category>> _useCase = useCase;

    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V3);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([AsParameters] PaginationRequestDto request, CancellationToken cancellationToken = default) =>
                    {
                        return await HandleUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("GetAllCategoriesV3")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V3)
            .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(10)))
            .WithDescription("Returns all categories")
            .WithSummary("List categories")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V3}" })
            .Produces<RequestResult<IEnumerable<Category>>>();

        return Task.FromResult(RequestResult.Success());
    }
}
