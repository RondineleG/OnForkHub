namespace OnForkHub.Application.Dtos.Base;

/// <summary>
/// Data transfer object for paginated results.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public sealed class PagedResultDto<T>
{
    /// <summary>
    /// Gets or sets the items in the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items.
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalItems / PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether there is a previous page.
    /// </summary>
    public bool HasPreviousPage => Page > 1;

    /// <summary>
    /// Gets a value indicating whether there is a next page.
    /// </summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>
    /// Creates a new paged result.
    /// </summary>
    /// <param name="items">The items in the current page.</param>
    /// <param name="page">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalItems">The total number of items.</param>
    /// <returns>A new paged result.</returns>
    public static PagedResultDto<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        return new PagedResultDto<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
        };
    }
}
