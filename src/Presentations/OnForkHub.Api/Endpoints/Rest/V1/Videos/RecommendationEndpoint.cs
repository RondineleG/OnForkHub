namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for video recommendations.
/// </summary>
public sealed partial class RecommendationEndpoint(ILogger<RecommendationEndpoint> logger, IRecommendationService recommendationService)
    : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/recommendations";

    private readonly ILogger<RecommendationEndpoint> _logger = logger;
    private readonly IRecommendationService _recommendationService = recommendationService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                Route,
                async (ClaimsPrincipal user, [FromQuery] int count = 10, CancellationToken cancellationToken = default) =>
                {
                    return await HandleGetRecommendationsAsync(user, count, cancellationToken);
                }
            )
            .WithName("GetVideoRecommendationsV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Gets a list of recommended videos for the current user or trending videos if not authenticated")
            .WithSummary("Get recommendations")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<IEnumerable<VideoResponse>>(StatusCodes.Status200OK);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Getting recommendations for user {UserId}, count {Count}")]
    private partial void LogGettingRecommendations(string? userId, int count);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Failed to get recommendations: {Message}")]
    private partial void LogRecommendationsFailed(string? message);

    private async Task<IResult> HandleGetRecommendationsAsync(ClaimsPrincipal user, int count, CancellationToken cancellationToken)
    {
        var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        LogGettingRecommendations(userId, count);

        var result = string.IsNullOrEmpty(userId)
            ? await _recommendationService.GetTrendingVideosAsync(count)
            : await _recommendationService.GetRecommendationsAsync(userId, count);

        if (result.Status != EResultStatus.Success)
        {
            LogRecommendationsFailed(result.Message);
            return Results.BadRequest(new { error = result.Message ?? "Failed to get recommendations" });
        }

        return Results.Ok(result.Data ?? []);
    }
}
