namespace OnForkHub.Core.Validations.Categories;

public class CategoryValidationService : ValidationService<Category>
{
    public CategoryValidationService(IValidationBuilder<Category> builder, IEntityValidator<Category> validator)
        : base(builder, validator)
    {
        // First mode validation
        AddValidation(category =>
        {
            var result = ValidationResult.Success();

            result.Merge(
                ValidateProperty(
                    category,
                    c => c.Name,
                    builder =>
                        builder.NotNull("Name is required").NotEmpty("Name cannot be empty").MaxLength(100, "Name cannot exceed 100 characters")
                )
            );

            if (!string.IsNullOrEmpty(category.Description))
            {
                result.Merge(
                    ValidateProperty(category, c => c.Description, builder => builder.MaxLength(200, "Description cannot exceed 200 characters"))
                );
            }

            return result;
        });

        AddValidation(category =>
        {
            var result = ValidationResult.Success();

            result.Merge(
                ValidateProperty(
                    category,
                    c => c.Name,
                    builder =>
                        builder.NotNull("Name is required").NotEmpty("Name cannot be empty").MaxLength(100, "Name cannot exceed 100 characters")
                )
            );

            if (!string.IsNullOrEmpty(category.Description))
            {
                result.Merge(
                    ValidateProperty(category, c => c.Description, builder => builder.MaxLength(200, "Description cannot exceed 200 characters"))
                );
            }

            return result;
        });

        // Second mode validation
        AddRule(new CategoryNameValidationRule());
        AddRule(new CategoryDescriptionValidationRule());

        WithErrorHandler(error => ValidationResult.Failure(error.Message, error.Field));
    }
}