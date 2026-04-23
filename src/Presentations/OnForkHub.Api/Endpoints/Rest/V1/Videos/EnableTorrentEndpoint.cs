namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for enabling torrent on a video.
/// </summary>
public sealed partial class EnableTorrentEndpoint(ILogger<EnableTorrentEndpoint> logger, IVideoService videoService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/{{id:guid}}/torrent";

    private readonly ILogger<EnableTorrentEndpoint> _logger = logger;

    private readonly IVideoService _videoService = videoService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [Authorize]
                async ([FromRoute] Guid id, [FromBody] EnableTorrentRequest request, CancellationToken cancellationToken) =>
                {
                    return await HandleEnableTorrentAsync(id, request, cancellationToken);
                }
            )
            .WithName("EnableVideoTorrentV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Enables torrent (P2P) for a specific video")
            .WithSummary("Enable video torrent")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Enabling torrent for video {VideoId} with magnet URI {MagnetUri}")]
    private partial void LogEnablingTorrent(Guid videoId, string magnetUri);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to enable torrent for video {VideoId}: {Message}")]
    private partial void LogEnableTorrentFailed(Guid videoId, string? message);

    private async Task<IResult> HandleEnableTorrentAsync(Guid id, EnableTorrentRequest request, CancellationToken cancellationToken)
    {
        LogEnablingTorrent(id, request.MagnetUri);

        var result = await _videoService.EnableTorrentAsync(id, request.MagnetUri);

        if (result.Status != EResultStatus.Success)
        {
            if (result.Status == EResultStatus.EntityNotFound)
            {
                return Results.NotFound(new { error = "Video not found", videoId = id });
            }

            LogEnableTorrentFailed(id, result.Message);
            return Results.BadRequest(new { error = result.Message ?? "Failed to enable torrent" });
        }

        return Results.Ok(new { success = true, videoId = id });
    }
}
