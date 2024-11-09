namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryRequestDto
{
    [Required(ErrorMessage = $"The {nameof(Name)} field is required")]
    [MaxLength(50, ErrorMessage = $"The {nameof(Name)} field must be at most 50 characters long.")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public static RequestResult<Core.Entities.Category> Create(CategoryRequestDto request)
    {
        var name = Core.ValueObjects.Name.Create(request.Name);
        return Core.Entities.Category.Create(name, request.Description);
    }

    public static RequestResult<Core.Entities.Category> Update(CategoryRequestDto request, Core.Entities.Category category)
    {
        var name = Core.ValueObjects.Name.Create(request.Name);
        category.UpdateCategory(name, request.Description);

        return category == null
            ? RequestResult<Core.Entities.Category>.WithError("Category not found")
            : RequestResult<Core.Entities.Category>.Success(category);
    }
}
