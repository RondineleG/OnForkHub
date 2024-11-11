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

    public override CustomValidationResult Validate()
    {
        var validationResult = new CustomValidationResult();
        CustomValidationResult.ThrowErrorIf(() => Id <= 0, BaseEntityResources.IdGreaterThanZero);
        return validationResult;
    }
}
