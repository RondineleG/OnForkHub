using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Core.Validations;

public class ValidationServiceFactory : IValidationServiceFactory
{
    public ICategoryValidationService CreateCategoryValidationService()
    {
        return new CategoryValidationService();
    }
}
