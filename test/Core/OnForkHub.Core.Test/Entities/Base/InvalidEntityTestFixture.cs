namespace OnForkHub.Core.Test.Entities.Base;

public class InvalidEntityTestFixture : BaseEntity
{
    public InvalidEntityTestFixture()
    {
    }

    public InvalidEntityTestFixture(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt)
    {
    }

    public void ForceValidation()
    {
        ValidateEntityState();
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();
        throw new DomainException("Invalid entity state");
    }
}