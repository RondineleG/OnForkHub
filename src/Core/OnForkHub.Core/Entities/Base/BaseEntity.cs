namespace OnForkHub.Core.Entities.Base;

public abstract class BaseEntity : IAggregateRoot
{
    protected BaseEntity()
    {
        CreatedAt = DateTime.UtcNow;
        ValidateInitialState();
    }

    protected BaseEntity(long id, DateTime createdAt, DateTime? updatedAt = null)
    {
        ValidateConstructorParameters(id, createdAt, updatedAt);

        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public DateTime CreatedAt { get; protected set; }
    public long Id { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }

    protected void Update()
    {
        UpdatedAt = DateTime.UtcNow;
        ValidateEntityState();
    }

    private void ValidateInitialState()
    {
        var validationResult = new CustomValidationResult();

        validationResult.AddErrorIf(
            CreatedAt == default || CreatedAt.Kind != DateTimeKind.Utc,
            "CreatedAt must be a valid UTC date",
            nameof(CreatedAt)
        );

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }

    private static void ValidateConstructorParameters(long id, DateTime createdAt, DateTime? updatedAt)
    {
        var validationResult = new CustomValidationResult();

        validationResult.AddErrorIf(id < 0, "Id cannot be negative", nameof(Id));

        validationResult
            .AddErrorIf(createdAt == default, "CreatedAt is required", nameof(CreatedAt))
            .AddErrorIf(createdAt.Kind != DateTimeKind.Utc, "CreatedAt must be UTC", nameof(CreatedAt));

        if (updatedAt.HasValue)
        {
            validationResult
                .AddErrorIf(updatedAt.Value.Kind != DateTimeKind.Utc, "UpdatedAt must be UTC", nameof(UpdatedAt))
                .AddErrorIf(updatedAt.Value <= createdAt, "UpdatedAt must be greater than CreatedAt", nameof(UpdatedAt));
        }

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }

    protected virtual void ValidateEntityState()
    {
        var validationResult = new CustomValidationResult();

        validationResult.AddErrorIf(
            CreatedAt == default || CreatedAt.Kind != DateTimeKind.Utc,
            "CreatedAt must be a valid UTC date",
            nameof(CreatedAt)
        );

        if (UpdatedAt.HasValue)
        {
            validationResult
                .AddErrorIf(UpdatedAt.Value.Kind != DateTimeKind.Utc, "UpdatedAt must be UTC", nameof(UpdatedAt))
                .AddErrorIf(UpdatedAt.Value <= CreatedAt, "UpdatedAt must be greater than CreatedAt", nameof(UpdatedAt));
        }

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }
}
