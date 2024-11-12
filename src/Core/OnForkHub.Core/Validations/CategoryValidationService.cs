using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Core.Validations;

public class CategoryValidationService : BaseValidationService, ICategoryValidationService
{
    public CustomValidationResult ValidateCategory(Category category)
    {
        var validationResult = new CustomValidationResult();

        if (category is null)
        {
            validationResult.AddError("Category cannot be null", nameof(Category));
            return validationResult;
        }

        validationResult
            .AddErrorIfNullOrWhiteSpace(category.Name.Value, "Category name is required", nameof(category.Name))
            .AddErrorIf(category.Description?.Length > 200, "Description cannot exceed 200 characters", nameof(category.Description));

        // Merge com a validação própria da entidade
        validationResult.Merge(category.Validate());

        return validationResult;
    }

    public CustomValidationResult ValidateCategoryUpdate(Category category)
    {
        var validationResult = ValidateCategory(category);

        // Validações específicas para atualização
        if (category.Id <= 0)
        {
            validationResult.AddError("Category ID is required for updates", nameof(category.Id));
        }

        return validationResult;
    }
}
