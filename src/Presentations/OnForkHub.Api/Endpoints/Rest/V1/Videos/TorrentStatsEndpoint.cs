namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for retrieving P2P torrent statistics.
/// </summary>
public sealed partial class TorrentStatsEndpoint(ILogger<TorrentStatsEndpoint> logger, IVideoService videoService, ITorrentTrackerService tracker)
    : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/{{id:guid}}/torrent/stats";

    private readonly ILogger<TorrentStatsEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;
    private readonly ITorrentTrackerService _tracker = tracker;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                Route,
                async ([FromRoute] Guid id, CancellationToken cancellationToken) =>
                {
                    return await HandleGetStatsAsync(id, cancellationToken);
                }
            )
            .WithName("GetVideoTorrentStatsV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Gets real-time P2P statistics for a video torrent")
            .WithSummary("Get torrent stats")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting torrent stats for video {VideoId}")]
    private partial void LogGettingStats(Guid videoId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get torrent stats for video {VideoId}: {Message}")]
    private partial void LogGetStatsFailed(Guid videoId, string? message);

    private async Task<IResult> HandleGetStatsAsync(Guid id, CancellationToken cancellationToken)
    {
        LogGettingStats(id);

        var videoResult = await _videoService.GetByIdAsync(id.ToString());
        if (
            videoResult.Status != EResultStatus.Success
            || videoResult.Data is null
            || !videoResult.Data.IsTorrentEnabled
            || videoResult.Data.MagnetUri == null
        )
        {
            return Results.NotFound();
        }

        var stats = await _tracker.GetStatsAsync(videoResult.Data.MagnetUri);

        return Results.Ok(
            new
            {
                videoId = id,
                peerCount = stats.PeerCount,
                seedCount = stats.SeedCount,
                leechCount = stats.LeechCount,
                healthScore = stats.HealthScore,
            }
        );
    }
}
