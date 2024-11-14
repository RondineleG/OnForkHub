namespace OnForkHub.Core.Test.Entities.Base;

public class InvalidEntityTestFixture : BaseEntity
{
    public InvalidEntityTestFixture()
        : base() { }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();
        throw new DomainException("Invalid entity state");
    }
}
