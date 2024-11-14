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
}
