// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Test.Entities.Base;

public class ValidEntityTestFixture : BaseEntity
{
    public ValidEntityTestFixture() { }

    public ValidEntityTestFixture(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt)
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIf(() => id is null, "Id cannot be null", nameof(Id));

        if (validationResult.HasError)
        {
            throw new DomainException(validationResult.ErrorMessage);
        }
    }

    public static void ExecuteException()
    {
        throw new DomainException("Invalid entity state");
    }

    public void ExecuteUpdate()
    {
        if (CreatedAt.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("CreatedAt must be UTC");
        }

        Update();
    }
}
