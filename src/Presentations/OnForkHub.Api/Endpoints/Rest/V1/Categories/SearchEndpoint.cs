namespace OnForkHub.Api.Endpoints.Rest.V1.Categories;

using OnForkHub.Application.Dtos.Base;
using OnForkHub.Application.Dtos.Category.Request;
using OnForkHub.Application.Dtos.Category.Response;
using OnForkHub.Core.Interfaces.Configuration;

/// <summary>
/// Endpoint for searching categories.
/// </summary>
public class SearchEndpoint(ILogger<SearchEndpoint> logger, IUseCase<CategorySearchRequestDto, PagedResultDto<CategoryResponseDto>> useCase)
    : BaseEndPoint<Category>,
        IEndpointAsync
{
    private const int V1 = 1;

    private static readonly string Route = GetVersionedRoute(V1) + "/search";

    private readonly ILogger<SearchEndpoint> _logger = logger;

    private readonly IUseCase<CategorySearchRequestDto, PagedResultDto<CategoryResponseDto>> _useCase = useCase;

    /// <inheritdoc/>
    public Task<RequestResult> RegisterAsync(WebApplication app)
    {
        var apiVersionSet = CreateApiVersionSet(app, V1);

        ConfigureEndpoint(
                app.MapGet(
                    Route,
                    async (
                        [FromQuery] string? searchTerm,
                        [FromQuery] CategorySortField sortBy = CategorySortField.Name,
                        [FromQuery] bool sortDescending = false,
                        [FromQuery] int page = 1,
                        [FromQuery] int itemsPerPage = 10,
                        CancellationToken cancellationToken = default
                    ) =>
                    {
                        var request = new CategorySearchRequestDto
                        {
                            SearchTerm = searchTerm,
                            SortBy = sortBy,
                            SortDescending = sortDescending,
                            Page = page,
                            ItemsPerPage = itemsPerPage,
                        };
                        return await HandleUseCase(_useCase, _logger, request, cancellationToken);
                    }
                )
            )
            .WithName("SearchCategoriesV1")
            .WithApiVersionSet(apiVersionSet)
            .MapToApiVersion(V1)
            .WithDescription("Searches categories with filters and pagination")
            .WithSummary("Search categories")
            .WithMetadata(new ApiExplorerSettingsAttribute { GroupName = $"v{V1}" })
            .Produces<RequestResult<PagedResultDto<CategoryResponseDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        return Task.FromResult(RequestResult.Success());
    }
}
