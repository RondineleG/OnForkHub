using OnForkHub.Core.Entities.Base;

namespace OnForkHub.Core.Test.Entities.Base;


public class EntidadeTesteValidaTest : BaseEntity
{
    public EntidadeTesteValidaTest() : base() { }

    public EntidadeTesteValidaTest(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public override void Validate()
    {
    }

    public void ExecutarUpdate()
    {
        Update();
    }
}

