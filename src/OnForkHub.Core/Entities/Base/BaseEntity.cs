namespace OnForkHub.Core;

public abstract class BaseEntity : IAggregateRoot
{
    public long Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
    }

    protected BaseEntity(long id, DateTime createdAt, DateTime? updatedAt = null)
    {
        DomainException.When(id <= 0, "Id deve ser maior que zero");
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public abstract void Validate();

    protected void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}

