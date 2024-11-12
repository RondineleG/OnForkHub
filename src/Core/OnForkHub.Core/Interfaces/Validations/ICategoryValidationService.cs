namespace OnForkHub.Core.Interfaces.Validations;

public interface ICategoryValidationService
{
    CustomValidationResult ValidateCategory(Category category);
    CustomValidationResult ValidateCategoryUpdate(Category category);
}
