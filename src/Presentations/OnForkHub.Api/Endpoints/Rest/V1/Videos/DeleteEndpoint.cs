namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for deleting a video.
/// </summary>
public class DeleteEndpoint(ILogger<DeleteEndpoint> logger, IUseCase<string, Video> useCase) : BaseEndPoint<Video>, IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/{id}";

    private readonly ILogger<DeleteEndpoint> _logger = logger;

    private readonly IUseCase<string, Video> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapDelete(
                    Route,
                    async ([FromRoute] string id, CancellationToken cancellationToken) =>
                    {
                        return await HandleDeleteUseCase(_useCase, _logger, id);
                    }
                )
            )
            .WithName("DeleteVideoV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Deletes a video by ID")
            .WithSummary("Delete video")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }
}
