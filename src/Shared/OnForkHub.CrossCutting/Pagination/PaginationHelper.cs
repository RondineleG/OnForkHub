namespace OnForkHub.CrossCutting.Pagination;

/// <summary>
/// Helper class for pagination operations.
/// </summary>
public static class PaginationHelper
{
    /// <summary>
    /// Parses pagination parameters from query string values.
    /// </summary>
    /// <param name="page">The page number as a string.</param>
    /// <param name="pageSize">The page size as a string.</param>
    /// <returns>A PaginationParams instance with parsed or default values.</returns>
    public static PaginationParams ParseFromQuery(string? page, string? pageSize)
    {
        var parsedPage = 1;
        var parsedPageSize = PaginationParams.DefaultPageSize;

        if (!string.IsNullOrWhiteSpace(page) && int.TryParse(page, out var pageValue) && pageValue >= 1)
        {
            parsedPage = pageValue;
        }

        if (!string.IsNullOrWhiteSpace(pageSize) && int.TryParse(pageSize, out var pageSizeValue) && pageSizeValue >= 1)
        {
            parsedPageSize = Math.Min(pageSizeValue, PaginationParams.MaxPageSize);
        }

        return new PaginationParams { Page = parsedPage, PageSize = parsedPageSize };
    }

    /// <summary>
    /// Creates a paginated response from a collection of items.
    /// </summary>
    /// <typeparam name="T">The type of items.</typeparam>
    /// <param name="items">The items for the current page.</param>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <param name="totalItems">The total number of items across all pages.</param>
    /// <returns>A paginated response containing the items and pagination metadata.</returns>
    public static PaginatedResponse<T> CreateResponse<T>(IEnumerable<T> items, int currentPage, int pageSize, long totalItems)
    {
        var totalPages = (long)Math.Ceiling((double)totalItems / pageSize);

        return new PaginatedResponse<T>
        {
            Items = items,
            CurrentPage = currentPage,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasNextPage = currentPage < totalPages,
            HasPreviousPage = currentPage > 1,
        };
    }
}
