namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationServiceFactory
{
    ICategoryValidationService CreateCategoryValidationService();
}
