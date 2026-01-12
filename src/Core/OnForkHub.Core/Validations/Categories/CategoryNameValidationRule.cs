namespace OnForkHub.Core.Validations.Categories;

public class CategoryNameValidationRule : IValidationRule<Category>
{
    public string PropertyName => nameof(Category.Name);

    public IValidationResult Validate(Category entity)
    {
        return ValidationResult
            .Success()
            .AddErrorIf(() => entity.Name is null, "Name is required", PropertyName)
            .AddErrorIf(() => string.IsNullOrEmpty(entity.Name?.Value), "Name cannot be empty", PropertyName)
            .AddErrorIf(() => entity.Name?.Value.Length > 100, "Name cannot exceed 100 characters", PropertyName);
    }
}
