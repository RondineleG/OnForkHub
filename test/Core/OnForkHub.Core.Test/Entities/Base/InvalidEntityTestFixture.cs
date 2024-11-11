namespace OnForkHub.Core.Test.Entities.Base;

public class InvalidEntityTestFixture : BaseEntity
{
    public InvalidEntityTestFixture()
        : base() { }

    public InvalidEntityTestFixture(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public void ExecuteUpdate()
    {
        Validate();
        Update();
    }

    public override CustomValidationResult Validate()
    {
        var validationResult = new CustomValidationResult();
        CustomValidationResult.ThrowErrorIf(() => Id <= 0, BaseEntityResources.IdGreaterThanZero);
        return validationResult;
    }
}
