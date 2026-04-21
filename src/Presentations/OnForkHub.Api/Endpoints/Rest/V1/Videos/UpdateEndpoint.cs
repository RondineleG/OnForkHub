namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for updating a video.
/// </summary>
public class UpdateEndpoint(ILogger<UpdateEndpoint> logger, IUseCase<VideoUpdateRequestDto, Video> useCase) : BaseEndPoint<Video>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}";

    private readonly ILogger<UpdateEndpoint> _logger = logger;

    private readonly IUseCase<VideoUpdateRequestDto, Video> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPut(
                    Route,
                    async ([FromRoute] string id, [FromBody] VideoUpdateRequestDto request, CancellationToken cancellationToken) =>
                    {
                        request.Id = id;
                        return await HandleUseCase(_useCase, _logger, request);
                    }
                )
            )
            .WithName("UpdateVideoV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Updates an existing video")
            .WithSummary("Update video")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<Video>>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
