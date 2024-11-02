using OnForkHub.Core.Entities.Base;

namespace OnForkHub.Core.Test.Entities.Base;

public class EntidadeInvalidaTestFixture : BaseEntity
{
    public EntidadeInvalidaTestFixture() : base() { }

    public EntidadeInvalidaTestFixture(long id, DateTime createdAt, DateTime? updatedAt = null)
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
