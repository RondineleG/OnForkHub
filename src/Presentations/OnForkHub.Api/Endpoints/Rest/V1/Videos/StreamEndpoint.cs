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
                async ([FromRoute] Guid id, [FromQuery] string? quality, CancellationToken cancellationToken) =>
                {
                    return await HandleStreamAsync(id, quality, cancellationToken);
                }
            )
            .WithName("StreamVideoV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Streams video content with DASH support and quality selection")
            .WithSummary("Stream video")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status206PartialContent);

        app.MapGet(
                $"{Route}/manifest.mpd",
                async ([FromRoute] Guid id, HttpContext context) =>
                {
                    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
                    var manifest = DashManifestGenerator.GenerateManifest(baseUrl, id);
                    return Results.Content(manifest, "application/dash+xml");
                }
            )
            .WithName("GetVideoManifestV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithSummary("Get DASH manifest");

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Streaming video {VideoId} with quality {Quality}")]
    private partial void LogStreamingVideo(Guid videoId, string? quality);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to stream video {VideoId}: {Message}")]
    private partial void LogStreamFailed(Guid videoId, string? message);

    private async Task<IResult> HandleStreamAsync(Guid id, string? quality, CancellationToken cancellationToken)
    {
        LogStreamingVideo(id, quality);

        var videoResult = await _videoService.GetByIdAsync(id.ToString());
        if (videoResult.Status != EResultStatus.Success || videoResult.Data is null)
        {
            return Results.NotFound();
        }

        var video = videoResult.Data;

        // In a real scenario, we would use 'quality' to pick the right file
        // For now we use the main URL
        var fileResult = await _storageService.GetAsync(video.Url.Value, cancellationToken);

        if (fileResult.Status != EResultStatus.Success || fileResult.Data is null)
        {
            LogStreamFailed(id, fileResult.Message);
            return Results.NotFound();
        }

        return Results.Stream(
            fileResult.Data,
            contentType: "video/mp4",
            enableRangeProcessing: true,
            lastModified: video.UpdatedAt ?? video.CreatedAt
        );
    }
}
