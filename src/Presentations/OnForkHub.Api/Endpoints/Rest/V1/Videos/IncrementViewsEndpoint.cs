namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for incrementing video view count.
/// </summary>
public sealed partial class IncrementViewsEndpoint(
    ILogger<IncrementViewsEndpoint> logger,
    IVideoService videoService,
    IEntityFrameworkDataContext dbContext
) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/{{id:guid}}/views";

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                async ([FromRoute] Guid id, CancellationToken cancellationToken) =>
                {
                    return await HandleIncrementViewsAsync(id, cancellationToken);
                }
            )
            .WithName("IncrementVideoViewsV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Analytics")
            .WithDescription("Increments the view count for a specific video")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Incrementing views for video {VideoId}")]
    private static partial void LogIncrementingViews(ILogger logger, Guid videoId);

    private readonly ILogger<IncrementViewsEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;
    private readonly IEntityFrameworkDataContext _dbContext = dbContext;

    private async Task<IResult> HandleIncrementViewsAsync(Guid id, CancellationToken cancellationToken)
    {
        LogIncrementingViews(_logger, id);

        var videoResult = await _videoService.GetByIdAsync(id.ToString());
        if (videoResult.Status != EResultStatus.Success || videoResult.Data is null)
        {
            return Results.NotFound();
        }

        var video = videoResult.Data;
        video.IncrementViews();

        await _videoService.UpdateAsync(video);

        return Results.NoContent();
    }
}
