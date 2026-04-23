namespace OnForkHub.Core.Responses.Categories;

using OnForkHub.Core.Entities;

/// <summary>
/// Response containing category information.
/// </summary>
public sealed class CategoryResponse
{
    /// <summary>
    /// Gets or sets the category identifier.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the category description.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Creates a CategoryResponse from a Category entity.
    /// </summary>
    /// <param name="category">The category entity.</param>
    /// <returns>The category response.</returns>
    public static CategoryResponse FromEntity(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return new CategoryResponse
        {
            Id = category.Id.ToString(),
            Name = category.Name.Value,
            Description = category.Description,
        };
    }
}
