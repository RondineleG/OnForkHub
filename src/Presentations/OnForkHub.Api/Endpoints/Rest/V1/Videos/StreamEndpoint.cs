namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for video streaming with range support.
/// </summary>
public sealed partial class StreamEndpoint(ILogger<StreamEndpoint> logger, IVideoService videoService, IFileStorageService storageService)
    : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/{{id:guid}}/stream";

    private readonly ILogger<StreamEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;
    private readonly IFileStorageService _storageService = storageService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                Route,
                async ([FromRoute] Guid id, CancellationToken cancellationToken) =>
                {
                    return await HandleStreamAsync(id, cancellationToken);
                }
            )
            .WithName("StreamVideoV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Streams video content with HTTP Range support for seeking")
            .WithSummary("Stream video")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status206PartialContent)
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Streaming video {VideoId}")]
    private partial void LogStreamingVideo(Guid videoId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to stream video {VideoId}: {Message}")]
    private partial void LogStreamFailed(Guid videoId, string? message);

    private async Task<IResult> HandleStreamAsync(Guid id, CancellationToken cancellationToken)
    {
        LogStreamingVideo(id);

        var videoResult = await _videoService.GetByIdAsync(id.ToString());
        if (videoResult.Status != EResultStatus.Success || videoResult.Data is null)
        {
            return Results.NotFound();
        }

        var video = videoResult.Data;

        // For local storage, we can use Results.File with range processing
        // For Azure, we get the stream and use Results.Stream
        var fileResult = await _storageService.GetAsync(video.Url.Value, cancellationToken);

        if (fileResult.Status != EResultStatus.Success || fileResult.Data is null)
        {
            LogStreamFailed(id, fileResult.Message);
            return Results.NotFound();
        }

        // ASP.NET Core Minimal APIs support range processing automatically when using Results.File or Results.Stream with proper parameters
        return Results.Stream(
            fileResult.Data,
            contentType: "video/mp4",
            enableRangeProcessing: true,
            lastModified: video.UpdatedAt ?? video.CreatedAt
        );
    }
}
