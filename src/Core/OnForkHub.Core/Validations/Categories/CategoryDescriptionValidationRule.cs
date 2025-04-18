namespace OnForkHub.Core.Validations.Categories;

public class CategoryDescriptionValidationRule : IValidationRule<Category>
{
    public string PropertyName => nameof(Category.Description);

    public IValidationResult Validate(Category entity)
    {
        return ValidationResult
            .Success()
            .AddErrorIf(
                () => !string.IsNullOrEmpty(entity.Description) && entity.Description.Length > 200,
                "Description cannot exceed 200 characters",
                PropertyName
            );
    }
}