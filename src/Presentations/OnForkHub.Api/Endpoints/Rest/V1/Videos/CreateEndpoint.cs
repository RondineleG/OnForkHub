namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for creating videos.
/// </summary>
public class CreateEndpoint(ILogger<CreateEndpoint> logger, IUseCase<VideoCreateRequestDto, Video> useCase) : BaseEndPoint<Video>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<CreateEndpoint> _logger = logger;

    private readonly IUseCase<VideoCreateRequestDto, Video> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPost(
                    Route,
                    async ([FromBody] VideoCreateRequestDto request, CancellationToken cancellationToken) =>
                    {
                        return await HandleCreateUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("CreateVideoV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Creates a new video")
            .WithSummary("Create video")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Video>>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
