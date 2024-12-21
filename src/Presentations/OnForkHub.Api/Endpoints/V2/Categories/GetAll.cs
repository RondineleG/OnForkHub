using OnForkHub.Application.Dtos.Base;

namespace OnForkHub.Api.Endpoints.V2.Categories;

public class GetAll(ILogger<GetAll> logger, IUseCase<PaginationRequestDto, IEnumerable<Category>> useCase) : BaseEndpoint<Category>, IEndpointAsync
{
    private const int V2 = 2;
    private static readonly string Route = GetVersionedRoute(V2);
    private readonly ILogger<GetAll> _logger = logger;
    private readonly IUseCase<PaginationRequestDto, IEnumerable<Category>> _useCase = useCase;
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V2);

        ConfigureEndpoint(
            app.MapGet(
                Route,
                async (CancellationToken cancellationToken) =>
                {
                    var request = new PaginationRequestDto { Page = 1, ItemsPerPage = 10 };
                    return await HandleUseCase(_useCase, _logger, request);
                }))
            .WithName("GetAllCategoriesV2")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V2)
            .CacheOutput(x => x.Expire(TimeSpan.FromMinutes(10)))
            .WithDescription("Returns all categories")
            .WithSummary("List categories")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V2}" })
            .Produces<RequestResult<IEnumerable<Category>>>();

        return Task.FromResult(RequestResult.Success());
    }
}
