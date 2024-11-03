using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Entities.Base;

public abstract class BaseEntity : IAggregateRoot
{
    protected BaseEntity()
    {
        this.CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(long id, DateTime createdAt, DateTime? updatedAt = null)
    {
        DomainException.ThrowErrorWhen(() => id <= 0, "Id deve ser maior que zero");
        this.Id = id;
        this.CreatedAt = createdAt;
        this.UpdatedAt = updatedAt;
    }

    public DateTime CreatedAt { get; protected set; }

    public long Id { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    public abstract ValidationResult Validate();

    protected void Update()
    {
        this.UpdatedAt = DateTime.UtcNow;
    }
}
