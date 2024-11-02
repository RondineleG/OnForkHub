using OnForkHub.Core.Entities.Base;

namespace OnForkHub.Core.Test.Entities.Base;

public class EntidadeTesteInvalidaTest : BaseEntity
{
    public EntidadeTesteInvalidaTest() : base() { }

    public EntidadeTesteInvalidaTest(long id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    public override void Validate()
    {
        DomainException.When(Id <= 0, "Id deve ser maior que zero");
    }

    public void ExecutarUpdate()
    {
        Validate();
        Update();
    }
}
