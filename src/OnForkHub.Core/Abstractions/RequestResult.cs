using OnForkHub.Core.Abstractions.Base;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Abstractions;

public class RequestResult : IRequestValidations, IRequestError, IRequestEntityWarning
{
    public RequestResult()
    {
        Status = ECustomResultStatus.Success;
        ValidationResult = new ValidationResult();
    }

    internal Dictionary<string, List<string>>? _entityErrors;

    internal List<string>? _generalErrors;

    public DateTime Date { get; set; } = DateTime.Now;

    public Dictionary<string, List<string>> EntityErrors => _entityErrors ??= [];

    public RequestEntityWarning? EntityWarning { get; protected init; }

    public RequestError? RequestError { get; protected init; }

    public List<string> GeneralErrors => _generalErrors ??= [];

    public string Id { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public ECustomResultStatus Status { get; set; }

    public ValidationResult ValidationResult { get; protected set; }

    public IEnumerable<RequestValidation> Validations { get; protected init; } = Enumerable.Empty<RequestValidation>();

    public static RequestResult EntityAlreadyExists(string entity, object id, string description)
    {
        return CreateEntityError(entity, id, description, ECustomResultStatus.EntityAlreadyExists);
    }

    public static RequestResult EntityHasError(string entity, object id, string description)
    {
        return CreateEntityError(entity, id, description, ECustomResultStatus.EntityHasError);
    }

    public static RequestResult EntityNotFound(string entity, object id, string description)
    {
        return CreateEntityError(entity, id, description, ECustomResultStatus.EntityNotFound);
    }

    public static RequestResult Success()
    {
        return new RequestResult { Status = ECustomResultStatus.Success };
    }

    public static RequestResult WithError(string message)
    {
        return new RequestResult { Status = ECustomResultStatus.HasError, RequestError = new RequestError(message) };
    }

    public static RequestResult WithError(Exception exception)
    {
        return WithError(exception.Message);
    }

    public static RequestResult WithError(List<string> generalErrors)
    {
        return new RequestResult { Status = ECustomResultStatus.HasError, _generalErrors = generalErrors };
    }

    public static RequestResult WithError(Dictionary<string, List<string>> entityErrors)
    {
        return new RequestResult { Status = ECustomResultStatus.EntityHasError, _entityErrors = entityErrors };
    }

    public static RequestResult WithError(RequestError error)
    {
        return new RequestResult { Status = ECustomResultStatus.HasError, RequestError = error };
    }

    public static RequestResult WithNoContent()
    {
        return new RequestResult { Status = ECustomResultStatus.NoContent };
    }

    public static RequestResult WithValidationError(string errorMessage, string fieldName = "")
    {
        var result = new RequestResult { Status = ECustomResultStatus.HasValidation };
        result.ValidationResult.AddError(errorMessage, fieldName);
        return result;
    }

    public static RequestResult WithValidations(params RequestValidation[] validations)
    {
        var result = new RequestResult { Status = ECustomResultStatus.HasValidation };

        foreach (var validation in validations)
        {
            result.ValidationResult.AddError(validation.Description, validation.PropertyName);
        }

        return result;
    }

    public static RequestResult WithValidations(IEnumerable<RequestValidation> validations)
    {
        return WithValidations(validations.ToArray());
    }

    public static RequestResult WithValidations(string propertyName, string description)
    {
        return WithValidations(new RequestValidation(propertyName, description));
    }

    public static RequestResult WithValidations(params ValidationErrorMessage[] validations)
    {
        var result = new RequestResult { Status = ECustomResultStatus.HasValidation };
        result.ValidationResult.AddErrors(validations.Select(v => (v.Message, v.Field)));
        return result;
    }

    public void AddEntityError(string entity, string message)
    {
        Status = ECustomResultStatus.EntityHasError;
        if (!EntityErrors.TryGetValue(entity, out var value))
        {
            value = [];
            EntityErrors[entity] = value;
        }
        value.Add(message);
    }

    public void AddError(string message)
    {
        Status = ECustomResultStatus.HasError;
        GeneralErrors.Add(message);
    }

    public override string ToString()
    {
        var messages = new List<string>();

        if (GeneralErrors.Count != 0)
        {
            messages.AddRange(GeneralErrors);
        }

        foreach (var entityError in EntityErrors)
        {
            foreach (var error in entityError.Value)
            {
                messages.Add($"{entityError.Key}: {error}");
            }
        }

        if (ValidationResult?.Errors != null && ValidationResult.Errors.Count != 0)
        {
            foreach (var validationError in ValidationResult.Errors)
            {
                messages.Add($"{validationError.Field}: {validationError.Message}");
            }
        }

        return string.Join("; ", messages);
    }

    protected static RequestResult CreateEntityError(
        string entity,
        object id,
        string description,
        ECustomResultStatus status
    )
    {
        return new RequestResult { Status = status, EntityWarning = new RequestEntityWarning(entity, id, description) };
    }
}

public class RequestResult<T> : RequestResult, IRequestCustomResult<T>
{
    public T? Data { get; private init; }

    public static new RequestResult<T> EntityAlreadyExists(string entity, object id, string description)
    {
        return (RequestResult<T>)CreateEntityError(entity, id, description, ECustomResultStatus.EntityAlreadyExists);
    }

    public static new RequestResult<T> EntityHasError(string entity, object id, string description)
    {
        return (RequestResult<T>)CreateEntityError(entity, id, description, ECustomResultStatus.EntityHasError);
    }

    public static new RequestResult<T> EntityNotFound(string entity, object id, string description)
    {
        return (RequestResult<T>)CreateEntityError(entity, id, description, ECustomResultStatus.EntityNotFound);
    }

    public static implicit operator RequestResult<T>(T data)
    {
        return Success(data);
    }

    public static implicit operator RequestResult<T>(Exception ex)
    {
        return WithError(ex);
    }

    public static implicit operator RequestResult<T>(RequestValidation[] validations)
    {
        return WithValidations(validations);
    }

    public static implicit operator RequestResult<T>(RequestValidation validation)
    {
        return WithValidations(validation);
    }

    public static RequestResult<T> Success(T data)
    {
        return new RequestResult<T> { Data = data, Status = ECustomResultStatus.Success };
    }

    public static new RequestResult<T> WithError(string message)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasError, RequestError = new RequestError(message) };
    }

    public static new RequestResult<T> WithError(Exception exception)
    {
        return WithError(exception.Message);
    }

    public static new RequestResult<T> WithError(List<string> generalErrors)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasError, _generalErrors = generalErrors };
    }

    public static new RequestResult<T> WithError(Dictionary<string, List<string>> entityErrors)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.EntityHasError, _entityErrors = entityErrors };
    }

    public static new RequestResult<T> WithError(RequestError error)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasError, RequestError = error };
    }

    public static new RequestResult<T> WithNoContent()
    {
        return new RequestResult<T> { Status = ECustomResultStatus.NoContent };
    }

    public static new RequestResult<T> WithValidationError(string errorMessage, string fieldName = "")
    {
        var result = new RequestResult<T> { Status = ECustomResultStatus.HasValidation };
        result.ValidationResult.AddError(errorMessage, fieldName);
        return result;
    }

    public static new RequestResult<T> WithValidations(params RequestValidation[] validations)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasValidation, Validations = validations };
    }

    public static new RequestResult<T> WithValidations(string propertyName, string description)
    {
        return WithValidations(new RequestValidation(propertyName, description));
    }

    public static new RequestResult<T> WithValidations(params ValidationErrorMessage[] validations)
    {
        var result = new RequestResult<T> { Status = ECustomResultStatus.HasValidation };
        result.ValidationResult.AddErrors(validations.Select(v => (v.Message, v.Field)));
        return result;
    }
}
