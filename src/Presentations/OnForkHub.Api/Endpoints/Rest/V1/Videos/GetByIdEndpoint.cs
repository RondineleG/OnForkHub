namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for getting a video by ID.
/// </summary>
public class GetByIdEndpoint(ILogger<GetByIdEndpoint> logger, IUseCase<string, Video> useCase) : BaseEndPoint<Video>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}";

    private readonly ILogger<GetByIdEndpoint> _logger = logger;

    private readonly IUseCase<string, Video> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([FromRoute] string id, CancellationToken cancellationToken) =>
                    {
                        return await HandleUseCase(_useCase, _logger, id);
                    }
                )
            )
            .WithName("GetVideoByIdV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Gets a video by ID")
            .WithSummary("Get video by ID")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Video>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return Task.FromResult(RequestResult.Success());
    }
}
