using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Entities.Base;

public class ValidEntityTestFixture : BaseEntity
{
    public ValidEntityTestFixture()
        : base() { }

    public ValidEntityTestFixture(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public void ExecuteUpdate()
    {
        Update();
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        ValidationResult.ThrowErrorIf(() => Id <= 0, "Id must be greater than zero");
        return validationResult;
    }
}
