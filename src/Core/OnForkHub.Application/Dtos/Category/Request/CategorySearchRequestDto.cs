namespace OnForkHub.Application.Dtos.Category.Request;

/// <summary>
/// Data transfer object for category search requests.
/// </summary>
public sealed class CategorySearchRequestDto : PaginationRequestDto
{
    /// <summary>
    /// Gets or sets the search term for category name.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or sets the sort field.
    /// </summary>
    public CategorySortField SortBy { get; set; } = CategorySortField.Name;

    /// <summary>
    /// Gets or sets a value indicating whether gets or sets the sort direction.
    /// </summary>
    public bool SortDescending { get; set; }
}

/// <summary>
/// Available fields for sorting categories.
/// </summary>
public enum CategorySortField
{
    /// <summary>
    /// Sort by name.
    /// </summary>
    Name,

    /// <summary>
    /// Sort by creation date.
    /// </summary>
    CreatedAt,
}
