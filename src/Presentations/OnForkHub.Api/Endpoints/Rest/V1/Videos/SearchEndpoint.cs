namespace OnForkHub.Api.Endpoints.Rest.V1.Videos;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Video.Request;
using OnForkHub.Application.Dtos.Video.Response;
using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for searching videos.
/// </summary>
public class SearchEndpoint(ILogger<SearchEndpoint> logger, IUseCase<VideoSearchRequestDto, PagedResultDto<VideoResponseDto>> useCase)
    : BaseEndPoint<Video>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/search";

    private readonly ILogger<SearchEndpoint> _logger = logger;

    private readonly IUseCase<VideoSearchRequestDto, PagedResultDto<VideoResponseDto>> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async (
                        [FromQuery] string? searchTerm,
                        [FromQuery] long? categoryId,
                        [FromQuery] string? userId,
                        [FromQuery] DateTime? fromDate,
                        [FromQuery] DateTime? toDate,
                        [FromQuery] VideoSortField sortBy = VideoSortField.CreatedAt,
                        [FromQuery] bool sortDescending = true,
                        [FromQuery] int page = 1,
                        [FromQuery] int itemsPerPage = 10,
                        CancellationToken cancellationToken = default
                    ) =>
                    {
                        var request = new VideoSearchRequestDto
                        {
                            SearchTerm = searchTerm,
                            CategoryId = categoryId,
                            UserId = userId,
                            FromDate = fromDate,
                            ToDate = toDate,
                            SortBy = sortBy,
                            SortDescending = sortDescending,
                            Page = page,
                            ItemsPerPage = itemsPerPage,
                        };
                        return await HandleUseCase(_useCase, _logger, request, cancellationToken);
                    }
                )
            )
            .WithName("SearchVideosV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Searches videos with filters and pagination")
            .WithSummary("Search videos")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<PagedResultDto<VideoResponseDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        return Task.FromResult(RequestResult.Success());
    }
}
