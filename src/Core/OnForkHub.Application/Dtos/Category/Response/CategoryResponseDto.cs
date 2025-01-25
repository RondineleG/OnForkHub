namespace OnForkHub.Application.Dtos.Category.Response;

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
}