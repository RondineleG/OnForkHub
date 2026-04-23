namespace OnForkHub.Web.Models;

/// <summary>
/// Represents a category DTO returned from the API.
/// </summary>
public class CategoryDto
{
    /// <summary>Gets or sets the category unique identifier.</summary>
    public long Id { get; set; }

    /// <summary>Gets or sets the category name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the category description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the number of videos in this category.</summary>
    public int VideoCount { get; set; }

    /// <summary>Gets or sets the creation date.</summary>
    public DateTime CreatedAt { get; set; }
}
