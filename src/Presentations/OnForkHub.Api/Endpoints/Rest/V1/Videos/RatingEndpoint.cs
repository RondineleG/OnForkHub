namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Enums;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Requests.Videos;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for managing video ratings (Like/Dislike).
/// </summary>
public sealed partial class RatingEndpoint(ILogger<RatingEndpoint> logger, IVideoService videoService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/{{id:guid}}/rating";

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapPost(
                Route,
                [Authorize]
                async ([FromRoute] Guid id, [FromBody] SetRatingRequest request, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleSetRatingAsync(id, request, user, cancellationToken);
                }
            )
            .WithName("SetVideoRatingV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Rating")
            .WithDescription("Sets a user's rating (Like/Dislike) for a video")
            .WithSummary("Set rating")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization();

        app.MapDelete(
                Route,
                [Authorize]
                async ([FromRoute] Guid id, ClaimsPrincipal user, CancellationToken cancellationToken) =>
                {
                    return await HandleRemoveRatingAsync(id, user, cancellationToken);
                }
            )
            .WithName("RemoveVideoRatingV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Rating")
            .WithDescription("Removes a user's rating for a video")
            .WithSummary("Remove rating")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Setting rating {Type} for video {VideoId} by user {UserId}")]
    private static partial void LogSettingRating(ILogger logger, ERatingType type, Guid videoId, string userId);

    private readonly ILogger<RatingEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;

    private static async Task<IResult> HandleRemoveRatingAsync(Guid videoId, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return await Task.FromResult(Results.NoContent());
    }

    private async Task<IResult> HandleSetRatingAsync(
        Guid videoId,
        SetRatingRequest request,
        ClaimsPrincipal user,
        CancellationToken cancellationToken
    )
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Results.Unauthorized();
        }

        LogSettingRating(_logger, request.Type, videoId, userId);

        // Logic placeholder
        return await Task.FromResult(Results.Ok(new { success = true }));
    }
}
