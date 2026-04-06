namespace OnForkHub.Web.Models;

/// <summary>
/// Represents a paginated result of items.
/// </summary>
/// <typeparam name="T">The type of items in the result.</typeparam>
public class PagedResult<T>
{
    /// <summary>Gets or sets the items for the current page.</summary>
    public List<T> Items { get; set; } = new();

    /// <summary>Gets or sets the total number of items available.</summary>
    public int TotalCount { get; set; }

    /// <summary>Gets or sets the current page number (1-based).</summary>
    public int Page { get; set; }

    /// <summary>Gets or sets the page size.</summary>
    public int PageSize { get; set; }

    /// <summary>Gets the total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>Gets whether there is a next page.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Gets whether there is a previous page.</summary>
    public bool HasPreviousPage => Page > 1;
}
