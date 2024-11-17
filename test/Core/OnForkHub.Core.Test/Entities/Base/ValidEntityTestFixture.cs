namespace OnForkHub.Core.Test.Entities.Base;

public class ValidEntityTestFixture : BaseEntity
{
    public ValidEntityTestFixture()
        : base() { }

    public ValidEntityTestFixture(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt)
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIf(id < 0, "Id cannot be negative", nameof(Id));

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }

    public void ExecuteUpdate()
    {
        if (CreatedAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("CreatedAt must be UTC");
        }
        Update();
    }

    public void ExecuteException()
    {
        throw new DomainException("Invalid entity state");
    }
}
