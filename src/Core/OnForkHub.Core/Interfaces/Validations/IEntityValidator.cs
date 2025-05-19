// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Interfaces.Validations;

public interface IEntityValidator<in T>
    where T : BaseEntity
{
    IValidationResult Validate(T entity);

    IValidationResult ValidateUpdate(T entity);
}
