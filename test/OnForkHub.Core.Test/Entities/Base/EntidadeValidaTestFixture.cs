using OnForkHub.Core.Entities.Base;

namespace OnForkHub.Core.Test.Entities.Base;


public class EntidadeValidaTestFixture : BaseEntity
{
    public EntidadeValidaTestFixture() : base() { }

    public EntidadeValidaTestFixture(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public override void Validate()
    {
    }

    public void ExecutarUpdate()
    {
        Update();
    }
}

