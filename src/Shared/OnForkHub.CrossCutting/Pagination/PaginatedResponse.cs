namespace OnForkHub.CrossCutting.Pagination;

/// <summary>
/// Represents a paginated response containing a collection of items.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public sealed class PaginatedResponse<T>
{
    /// <summary>
    /// Gets the collection of items for the current page.
    /// </summary>
    public IEnumerable<T> Items { get; init; } = [];

    /// <summary>
    /// Gets the current page number (1-based).
    /// </summary>
    public int CurrentPage { get; init; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// Gets the total number of items across all pages.
    /// </summary>
    public long TotalItems { get; init; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public long TotalPages { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage { get; init; }
}
