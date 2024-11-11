namespace OnForkHub.Application.Dtos.Category.Response;

public class CategoryResponseDto
{
    /// <summary>
    /// Category ID.
    /// </summary>
    /// <example>1</example>
    public long Id { get; set; }

    /// <summary>
    /// Category name.
    /// </summary>
    /// <example>Animation</example>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of category.
    /// </summary>
    /// <example>This example represents an animation category.</example>
    public string Description { get; set; } = string.Empty;
}
