using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Core.Validations.Base;

public abstract class BaseValidationService : IValidationService
{
    public virtual void AddErrorIfInvalid(CustomValidationResult validationResult, string context, RequestResult requestResult)
    {
        if (validationResult.HasError)
        {
            requestResult.Status = EResultStatus.HasValidation;
            foreach (var error in validationResult.Errors)
            {
                requestResult.AddEntityError(context, error.Message);
            }
        }
    }

    public virtual void Validate<T>(T entity, Func<T, CustomValidationResult> funcValidationResult, string entityName, RequestResult requestResult)
    {
        if (entity is null)
        {
            requestResult.AddEntityError(entityName, $"{entityName} cannot be null");
            return;
        }

        var validationResult = funcValidationResult(entity);
        AddErrorIfInvalid(validationResult, entityName, requestResult);
    }

    public virtual void Validate<T>(
        IEnumerable<T> entities,
        Func<T, CustomValidationResult> funcValidationResult,
        string entityName,
        RequestResult requestResult
    )
    {
        if (entities is null)
        {
            requestResult.AddEntityError(entityName, $"Collection of {entityName} cannot be null");
            return;
        }

        var index = 0;
        foreach (var entity in entities)
        {
            if (entity is null)
            {
                requestResult.AddEntityError(entityName, $"{entityName} at index {index} cannot be null");
                continue;
            }

            var validationResult = funcValidationResult(entity);
            if (validationResult.HasError)
            {
                foreach (var error in validationResult.Errors)
                {
                    requestResult.AddEntityError(entityName, $"Error at index {index}: {error.Message}");
                }
            }

            index++;
        }
    }

    public virtual CustomValidationResult Validate(string value, string regexPattern, string fieldName)
    {
        var validationResult = new CustomValidationResult();

        validationResult.AddErrorIfNullOrWhiteSpace(value, $"The field {fieldName} cannot be empty", fieldName);

        if (!string.IsNullOrWhiteSpace(value))
        {
            try
            {
                var regex = new Regex(regexPattern);
                if (!regex.IsMatch(value))
                {
                    validationResult.AddError($"The field {fieldName} does not match the required pattern", fieldName);
                }
            }
            catch (ArgumentException)
            {
                validationResult.AddError($"Invalid regex pattern for field {fieldName}", fieldName);
            }
        }

        return validationResult;
    }

    public virtual CustomValidationResult Validate(DateTime dateTimeStart, DateTime dateTimeFinish, string fieldName)
    {
        var validationResult = new CustomValidationResult();

        if (dateTimeStart == default)
        {
            validationResult.AddError($"The start date for {fieldName} cannot be empty", fieldName);
        }

        if (dateTimeFinish == default)
        {
            validationResult.AddError($"The end date for {fieldName} cannot be empty", fieldName);
        }

        if (dateTimeStart != default && dateTimeFinish != default)
        {
            if (dateTimeStart > dateTimeFinish)
            {
                validationResult.AddError($"The start date for {fieldName} must be before the end date", fieldName);
            }
        }

        return validationResult;
    }
}
