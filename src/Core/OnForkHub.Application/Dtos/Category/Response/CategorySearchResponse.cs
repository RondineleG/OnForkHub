namespace OnForkHub.Application.Dtos.Category.Response;

/// <summary>
/// Data transfer object for category responses.
/// </summary>
public class CategorySearchResponse
{
    /// <summary>Gets or sets the description of the category.</summary>
    /// <example>This example represents an animation category.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the category ID.</summary>
    /// <example>b7c2e8c4f0a34f1f9d3a5c6e7b8d9e0f.</example>
    public string Id { get; set; } = string.Empty;

    /// <summary>Gets or sets the category name.</summary>
    /// <example>Animation.</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the creation date.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the last update date.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Creates a CategorySearchResponse from a Category entity.
    /// </summary>
    /// <param name="category">The category entity.</param>
    /// <returns>The category response DTO.</returns>
    public static CategorySearchResponse FromCategory(Core.Entities.Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return new CategorySearchResponse
        {
            Id = category.Id,
            Name = category.Name.Value,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
        };
    }
}
