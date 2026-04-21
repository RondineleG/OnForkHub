namespace OnForkHub.Application.Dtos.Category.Response;

/// <summary>
/// Data transfer object for category responses.
/// </summary>
public class CategoryResponseDto
{
    /// <summary>Gets or sets the description of the category.</summary>
    /// <example>This example represents an animation category.</example>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the category ID.</summary>
    /// <example>1.</example>
    public long Id { get; set; }

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
    /// Creates a CategoryResponseDto from a Category entity.
    /// </summary>
    /// <param name="category">The category entity.</param>
    /// <returns>The category response DTO.</returns>
    public static CategoryResponseDto FromCategory(Core.Entities.Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        // Extract numeric ID from the string format "CollectionName/NumericId"
        var idParts = category.Id.Split('/');
        var numericId = idParts.Length > 1 && long.TryParse(idParts[1], out var id) ? id : 0;

        return new CategoryResponseDto
        {
            Id = numericId,
            Name = category.Name.Value,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt,
        };
    }
}
