namespace OnForkHub.CrossCutting.Pagination;

/// <summary>
/// Represents pagination parameters for querying data.
/// </summary>
public sealed class PaginationParams
{
    /// <summary>
    /// The minimum allowed page size.
    /// </summary>
    public const int MinPageSize = 1;

    /// <summary>
    /// The maximum allowed page size.
    /// </summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// The default page size.
    /// </summary>
    public const int DefaultPageSize = 20;

    /// <summary>
    /// Gets or sets the current page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Gets or sets the number of items per page.
    /// </summary>
    public int PageSize { get; set; } = DefaultPageSize;

    /// <summary>
    /// Validates the pagination parameters.
    /// </summary>
    /// <returns>A validation result indicating whether the parameters are valid.</returns>
    public ValidationResult Validate()
    {
        if (Page < 1)
        {
            return ValidationResult.Failure("Page number must be >= 1");
        }

        if (PageSize < MinPageSize)
        {
            return ValidationResult.Failure($"Page size must be >= {MinPageSize}");
        }

        if (PageSize > MaxPageSize)
        {
            return ValidationResult.Failure($"Page size cannot exceed {MaxPageSize}");
        }

        return ValidationResult.Success;
    }

    /// <summary>
    /// Calculates the number of items to skip based on the current page and page size.
    /// </summary>
    /// <returns>The number of items to skip.</returns>
    public int GetSkipCount() => (Page - 1) * PageSize;
}
