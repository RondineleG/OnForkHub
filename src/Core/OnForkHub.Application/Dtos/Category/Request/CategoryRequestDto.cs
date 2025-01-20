namespace OnForkHub.Application.Dtos.Category.Request;

public class CategoryRequestDto
{
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = $"The {nameof(Name)} field is required")]
    [MaxLength(50, ErrorMessage = $"The {nameof(Name)} field must be at most 50 characters long.")]
    public string Name { get; set; } = string.Empty;

    public static RequestResult<Core.Entities.Category> Create(CategoryRequestDto request)
    {
        var name = Core.ValueObjects.Name.Create(request.Name);
        return Core.Entities.Category.Create(name, request.Description);
    }

    public static RequestResult<Core.Entities.Category> Update(CategoryRequestDto request, Core.Entities.Category category)
    {
        if (request is null)
        {
            return RequestResult<Core.Entities.Category>.WithError("Request cannot be null");
        }

        if (category is null)
        {
            return RequestResult<Core.Entities.Category>.WithError("Category not found");
        }

        try
        {
            var name = Core.ValueObjects.Name.Create(request.Name);
            var updateResult = category.UpdateCategory(name, request.Description);

            return updateResult.Status == EResultStatus.Success
                ? RequestResult<Core.Entities.Category>.Success(category)
                : RequestResult<Core.Entities.Category>.WithError(updateResult.Message);
        }
        catch (DomainException ex)
        {
            return RequestResult<Core.Entities.Category>.WithError(ex.Message);
        }
    }
}
