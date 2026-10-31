namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Responses;
using OnForkHub.CrossCutting.Interfaces;

/// <summary>
/// Endpoint for advanced video search and filtering.
/// </summary>
public sealed partial class SearchVideosEndpoint(ILogger<SearchVideosEndpoint> logger, IVideoService videoService) : IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = $"/api/v{V1}/videos/search";

    private readonly ILogger<SearchVideosEndpoint> _logger = logger;
    private readonly IVideoService _videoService = videoService;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = app.NewApiVersionSet().HasApiVersion(new ApiVersion(V1)).ReportApiVersions().Build();

        app.MapGet(
                Route,
                async (
                    [FromQuery] string? q,
                    [FromQuery] long? categoryId,
                    [FromQuery] string? userId,
                    [FromQuery] DateTime? from,
                    [FromQuery] DateTime? to,
                    [FromQuery] int sortBy = 0,
                    [FromQuery] bool desc = true,
                    [FromQuery] int page = 1,
                    [FromQuery] int pageSize = 20,
                    CancellationToken cancellationToken = default
                ) =>
                {
                    return await HandleSearchAsync(q, categoryId, userId, from, to, sortBy, desc, page, pageSize, cancellationToken);
                }
            )
            .WithName("SearchVideosV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithTags("Video")
            .WithDescription("Advanced search for videos with multiple filters")
            .WithSummary("Search videos")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<SearchResponse<VideoResponse>>(StatusCodes.Status200OK);

        return Task.FromResult(RequestResult.Success());
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Searching videos with term {SearchTerm}")]
    private static partial void LogSearchingVideos(ILogger logger, string? searchTerm);

    private async Task<IResult> HandleSearchAsync(
        string? q,
        long? categoryId,
        string? userId,
        DateTime? from,
        DateTime? to,
        int sortBy,
        bool desc,
        int page,
        int pageSize,
        CancellationToken cancellationToken
    )
    {
        LogSearchingVideos(_logger, q);

        var result = await _videoService.SearchAsync(q, categoryId, userId, from, to, sortBy, desc, page, pageSize);

        if (result.Status != EResultStatus.Success)
        {
            return Results.BadRequest(new { error = result.Message ?? "Search failed" });
        }

        var (items, totalCount) = result.Data;
        var responses = items.Select(VideoResponse.FromVideo);

        return Results.Ok(
            new
            {
                items = responses,
                totalCount,
                page,
                pageSize,
            }
        );
    }
}
