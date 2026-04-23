namespace OnForkHub.Core.Responses;

/// <summary>
/// Represents a paginated search response.
/// </summary>
/// <typeparam name="T">The type of items in the response.</typeparam>
public sealed class SearchResponse<T>
{
    /// <summary>
    /// Gets or sets the list of items found.
    /// </summary>
    public List<T> Items { get; set; } = [];

    /// <summary>
    /// Gets or sets the total number of items matching the criteria.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int Page { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
