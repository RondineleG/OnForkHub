using OnForkHub.Core.Interfaces.Validations;
using OnForkHub.Core.Validations.Base;

namespace OnForkHub.Core.Validations;

public class CategoryValidationService(IValidationBuilder builder) : ValidationBase<Category>(builder)
{
    protected override IValidationResult ValidateEntity(Category entity)
    {
        var result = new CustomValidationResult();

        result.Merge(ValidateStringLength(entity.Name.Value, nameof(entity.Name), 100));

        if (entity.Description?.Length > 200)
        {
            result.AddError("Description cannot exceed 200 characters", nameof(entity.Description));
        }

        return result;
    }
}
