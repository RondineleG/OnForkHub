namespace OnForkHub.Core.Interfaces.Validations;

public interface ICategoryValidationService : IEntityValidator<Category>
{
    CustomValidationResult ValidateCategory(Category category);
    CustomValidationResult ValidateCategoryUpdate(Category category);
}
