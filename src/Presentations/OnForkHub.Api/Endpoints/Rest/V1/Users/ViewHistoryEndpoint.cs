namespace OnForkHub.Api.Endpoints.Rest.V1.Users;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for managing user's video viewing history.
/// </summary>
public sealed partial class ViewHistoryEndpoint(
    ILogger<ViewHistoryEndpoint> logger,
    IVideoService videoService,
    IEntityFrameworkDataContext dbContext
) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/users/history";

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                $"{Route}/{{videoId:guid}}",
                [Authorize]
                async ([FromRoute] Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleAddHistoryAsync(videoId, user, cancellationToken);
                }
            )
            .WithName("AddViewHistoryV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("History")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

        app.MapGet(
                Route,
                [Authorize]
                async (
                    ClaimsPrincipal user,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 20,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    return await HandleGetHistoryAsync(user, page, pageSize, cancellationToken);
                }
            )
            .WithName("GetUserHistoryV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("History")
            .Produces<IEnumerable<VideoResponse>>(StatusCodes.Status200OK);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Recording view history for user {UserId}, video {VideoId}")]
    private static partial void LogRecordingHistory(ILogger logger, string userId, Guid videoId);

    private readonly ILogger<ViewHistoryEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;
    private readonly IEntityFrameworkDataContext _dbContext = dbContext;

    private async Task<IResult> HandleAddHistoryAsync(Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        LogRecordingHistory(_logger, userId, videoId);

        var history = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _dbContext.ViewHistories,
            h => h.VideoId == videoId && h.UserId.ToString() == userId,
            cancellationToken
        );

        if (history != null)
        {
            history.UpdateLastViewed();
            _dbContext.ViewHistories.Update(history);
        }
        else
        {
            var historyResult = ViewHistory.Create(videoId, userId);
            if (historyResult.Status == EResultStatus.Success)
            {
                _dbContext.ViewHistories.Add(historyResult.Data!);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private async Task<IResult> HandleGetHistoryAsync(ClaimsPrincipal user, int page, int pageSize, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var videoIds = await EntityFrameworkQueryableExtensions.ToListAsync(
            _dbContext
                .ViewHistories.Where(h => h.UserId.ToString() == userId)
                .OrderByDescending(h => h.LastViewedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(h => h.VideoId),
            cancellationToken
        );

        var videos = new List<VideoResponse>();
        foreach (var id in videoIds)
        {
            var v = await _videoService.GetByIdAsync(id.ToString());
            if (v.Status == EResultStatus.Success && v.Data != null)
            {
                videos.Add(VideoResponse.FromVideo(v.Data));
            }
        }

        return Results.Ok(videos);
    }
}
