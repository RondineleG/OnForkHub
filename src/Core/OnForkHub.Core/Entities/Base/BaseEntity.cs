namespace OnForkHub.Core.Entities.Base;

public abstract class BaseEntity : IAggregateRoot
{
    protected BaseEntity()
    {
        _entityId = ValueObjects.Id.Create();
        Id = $"{GetCollectionName()}/{_entityId}";
        CreatedAt = DateTime.UtcNow;
        ValidateInitialState();
    }

    protected BaseEntity(Id id, DateTime createdAt, DateTime? updatedAt = null)
    {
        ValidateConstructorParameters(id, createdAt, updatedAt);
        _entityId = id;
        Id = $"{GetCollectionName()}/{id}";
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    private readonly Id _entityId;

    public DateTime CreatedAt { get; protected set; }

    public string Id { get; set; }

    public DateTime? UpdatedAt { get; protected set; }

    protected virtual string GetCollectionName()
    {
        return GetType().Name;
    }

    protected void Update()
    {
        UpdatedAt = DateTime.UtcNow;
        ValidateEntityState();
    }

    protected virtual void ValidateEntityState()
    {
        var result = ValidationResult
            .Success()
            .AddErrorIf(() => CreatedAt == default || CreatedAt.Kind != DateTimeKind.Utc, "CreatedAt must be a valid UTC date", nameof(CreatedAt));

        if (UpdatedAt.HasValue)
        {
            result
                .AddErrorIf(() => UpdatedAt.Value.Kind != DateTimeKind.Utc, "UpdatedAt must be UTC", nameof(UpdatedAt))
                .AddErrorIf(() => UpdatedAt.Value <= CreatedAt, "UpdatedAt must be greater than CreatedAt", nameof(UpdatedAt));
        }

        result.ThrowIfInvalid();
    }

    private static void ValidateConstructorParameters(Id id, DateTime createdAt, DateTime? updatedAt)
    {
        var result = ValidationResult
            .Success()
            .AddErrorIf(() => id == null, "Id cannot be null", nameof(Id))
            .AddErrorIf(() => createdAt == default, "CreatedAt is required", nameof(CreatedAt))
            .AddErrorIf(() => createdAt.Kind != DateTimeKind.Utc, "CreatedAt must be UTC", nameof(CreatedAt));

        if (updatedAt.HasValue)
        {
            result
                .AddErrorIf(() => updatedAt.Value.Kind != DateTimeKind.Utc, "UpdatedAt must be UTC", nameof(UpdatedAt))
                .AddErrorIf(() => updatedAt.Value <= createdAt, "UpdatedAt must be greater than CreatedAt", nameof(UpdatedAt));
        }

        result.ThrowIfInvalid();
    }

    private void ValidateInitialState()
    {
        ValidationResult
            .Success()
            .AddErrorIf(() => CreatedAt == default || CreatedAt.Kind != DateTimeKind.Utc, "CreatedAt must be a valid UTC date", nameof(CreatedAt))
            .AddErrorIf(() => Id == null, "Id cannot be null", nameof(Id))
            .ThrowIfInvalid();
    }
}
