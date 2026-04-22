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
/// Endpoint for managing user's favorite videos.
/// </summary>
public sealed partial class UserFavoriteEndpoint(
    ILogger<UserFavoriteEndpoint> logger,
    IVideoService videoService,
    IEntityFrameworkDataContext dbContext
) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/users/favorites";

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                $"{Route}/{{videoId:guid}}",
                [Authorize]
                async ([FromRoute] Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleAddFavoriteAsync(videoId, user, cancellationToken);
                }
            )
            .WithName("AddUserFavoriteV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Favorites")
            .Produces(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapDelete(
                $"{Route}/{{videoId:guid}}",
                [Authorize]
                async ([FromRoute] Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleRemoveFavoriteAsync(videoId, user, cancellationToken);
                }
            )
            .WithName("RemoveUserFavoriteV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Favorites")
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
                    return await HandleGetFavoritesAsync(user, page, pageSize, cancellationToken);
                }
            )
            .WithName("GetUserFavoritesV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Favorites")
            .Produces<IEnumerable<VideoResponse>>(StatusCodes.Status200OK);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} favoriting video {VideoId}")]
    private static partial void LogAddingFavorite(ILogger logger, string userId, Guid videoId);

    private readonly ILogger<UserFavoriteEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;
    private readonly IEntityFrameworkDataContext _dbContext = dbContext;

    private async Task<IResult> HandleAddFavoriteAsync(Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        LogAddingFavorite(_logger, userId, videoId);

        var videoResult = await _videoService.GetByIdAsync(videoId.ToString());
        if (videoResult.Status != EResultStatus.Success)
            return Results.NotFound();

        var favoriteResult = UserFavorite.Create(videoId, userId);
        if (favoriteResult.Status != EResultStatus.Success)
            return Results.BadRequest(favoriteResult.Message);

        _dbContext.UserFavorites.Add(favoriteResult.Data!);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Results.Created($"{Route}/{videoId}", new { success = true });
    }

    private async Task<IResult> HandleRemoveFavoriteAsync(Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var favorite = await EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            _dbContext.UserFavorites,
            f => f.VideoId == videoId && f.UserId.ToString() == userId,
            cancellationToken
        );

        if (favorite != null)
        {
            _dbContext.UserFavorites.Remove(favorite);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return Results.NoContent();
    }

    private async Task<IResult> HandleGetFavoritesAsync(ClaimsPrincipal user, int page, int pageSize, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Results.Unauthorized();

        var videoIds = await EntityFrameworkQueryableExtensions.ToListAsync(
            _dbContext
                .UserFavorites.Where(f => f.UserId.ToString() == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.VideoId),
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
