namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for getting all videos.
/// </summary>
public class GetAllEndpoint(ILogger<GetAllEndpoint> logger, IUseCase<PaginationRequestDto, IEnumerable<Video>> useCase)
    : BaseEndPoint<Video>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<GetAllEndpoint> _logger = logger;

    private readonly IUseCase<PaginationRequestDto, IEnumerable<Video>> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async ([FromQuery] int page = 1, [FromQuery] int itemsPerPage = 10, CancellationToken cancellationToken = default) =>
                    {
                        var request = new PaginationRequestDto { Page = page, ItemsPerPage = itemsPerPage };
                        return await HandleUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("GetAllVideosV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Gets all videos with pagination")
            .WithSummary("Get all videos")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<IEnumerable<Video>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        return Task.FromResult(RequestResult.Success());
    }
}
