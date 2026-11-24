namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for updating a video.
/// </summary>
public class UpdateEndpoint(ILogger<UpdateEndpoint> logger, IUseCase<VideoUpdateRequest, Video> useCase) : BaseEndPoint<Video>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}";

    private readonly ILogger<UpdateEndpoint> _logger = logger;

    private readonly IUseCase<VideoUpdateRequest, Video> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPut(
                    Route,
                    async ([FromRoute] string id, [FromBody] VideoUpdateRequest request, HttpContext httpContext, CancellationToken cancellationToken) =>
                    {
                        request.Id = id;
                        request.RequesterUserId = httpContext.User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                        request.RequesterIsAdmin = httpContext.User.IsInRole(CrossCutting.Authorization.Roles.Admin);
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
