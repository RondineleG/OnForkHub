using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Entities.Base;

public class EntidadeValidaTestFixture : BaseEntity
{
    public EntidadeValidaTestFixture()
        : base() { }

    public EntidadeValidaTestFixture(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public void ExecutarUpdate()
    {
        Update();
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        ValidationResult.ThrowErrorIf(() => Id <= 0, "Id deve ser maior que zero");
        return validationResult;
    }
}
