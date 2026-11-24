namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for creating videos.
/// </summary>
public class CreateEndpoint(ILogger<CreateEndpoint> logger, IUseCase<VideoCreateRequest, Video> useCase) : BaseEndPoint<Video>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1);

    private readonly ILogger<CreateEndpoint> _logger = logger;

    private readonly IUseCase<VideoCreateRequest, Video> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapPost(
                    Route,
                    async ([FromBody] VideoCreateRequest request, HttpContext httpContext, CancellationToken cancellationToken) =>
                    {
                        var userId = httpContext.User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                        if (string.IsNullOrWhiteSpace(userId))
                        {
                            return Results.Unauthorized();
                        }

                        request.UserId = userId;
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
