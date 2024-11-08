namespace OnForkHub.Core.Entities.Base;

public abstract class BaseEntity : IAggregateRoot
{
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(long id, DateTime createdAt, DateTime? updatedAt = null)
    {
        DomainException.ThrowErrorWhen(() => id <= 0, BaseEntityResources.IdGreaterThanZero);
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public DateTime CreatedAt { get; protected set; }

    public long Id { get; protected set; }

    public DateTime? UpdatedAt { get; protected set; }

    public abstract ValidationResult Validate();

    protected void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
