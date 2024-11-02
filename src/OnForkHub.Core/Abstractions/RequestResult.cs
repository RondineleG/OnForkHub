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

    protected Dictionary<string, List<string>>? _entityErrors;

    protected List<string>? _generalErrors;

    public DateTime Date { get; set; } = DateTime.Now;

    public Dictionary<string, List<string>> EntityErrors => _entityErrors ??= new Dictionary<string, List<string>>();

    public EntityWarning? EntityWarning { get; protected init; }

    public Error? Error { get; protected init; }

    public List<string> GeneralErrors => _generalErrors ??= new List<string>();

    public string Id { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public ECustomResultStatus Status { get; set; }

    public ValidationResult ValidationResult { get; protected set; }

    public IEnumerable<Validation> Validations { get; protected init; } = Enumerable.Empty<Validation>();

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
        return new RequestResult { Status = ECustomResultStatus.HasError, Error = new Error(message) };
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

    public static RequestResult WithError(Error error)
    {
        return new RequestResult { Status = ECustomResultStatus.HasError, Error = error };
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

    public static RequestResult WithValidations(params Validation[] validations)
    {
        return new RequestResult { Status = ECustomResultStatus.HasValidation, Validations = validations };
    }

    public static RequestResult WithValidations(IEnumerable<Validation> validations)
    {
        return WithValidations(validations.ToArray());
    }

    public static RequestResult WithValidations(string propertyName, string description)
    {
        return WithValidations(new Validation(propertyName, description));
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
            value = new List<string>();
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
        return string.Join("; ", messages);
    }

    protected static RequestResult CreateEntityError(string entity, object id, string description, ECustomResultStatus status)
    {
        return new RequestResult { Status = status, EntityWarning = new EntityWarning(entity, id, description) };
    }
}

public class RequestResult<T> : RequestResult, IRequestCustomResult<T>
{
    public T? Data { get; private init; }

    public new static RequestResult<T> EntityAlreadyExists(string entity, object id, string description)
    {
        return (RequestResult<T>)CreateEntityError(entity, id, description, ECustomResultStatus.EntityAlreadyExists);
    }

    public new static RequestResult<T> EntityHasError(string entity, object id, string description)
    {
        return (RequestResult<T>)CreateEntityError(entity, id, description, ECustomResultStatus.EntityHasError);
    }

    public new static RequestResult<T> EntityNotFound(string entity, object id, string description)
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

    public static implicit operator RequestResult<T>(Validation[] validations)
    {
        return WithValidations(validations);
    }

    public static implicit operator RequestResult<T>(Validation validation)
    {
        return WithValidations(validation);
    }

    public static RequestResult<T> Success(T data)
    {
        return new RequestResult<T> { Data = data, Status = ECustomResultStatus.Success };
    }

    public new static RequestResult<T> WithError(string message)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasError, Error = new Error(message) };
    }

    public new static RequestResult<T> WithError(Exception exception)
    {
        return WithError(exception.Message);
    }

    public new static RequestResult<T> WithError(List<string> generalErrors)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasError, _generalErrors = generalErrors };
    }

    public new static RequestResult<T> WithError(Dictionary<string, List<string>> entityErrors)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.EntityHasError, _entityErrors = entityErrors };
    }

    public new static RequestResult<T> WithError(Error error)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasError, Error = error };
    }

    public new static RequestResult<T> WithNoContent()
    {
        return new RequestResult<T> { Status = ECustomResultStatus.NoContent };
    }

    public new static RequestResult<T> WithValidationError(string errorMessage, string fieldName = "")
    {
        var result = new RequestResult<T> { Status = ECustomResultStatus.HasValidation };
        result.ValidationResult.AddError(errorMessage, fieldName);
        return result;
    }

    public new static RequestResult<T> WithValidations(params Validation[] validations)
    {
        return new RequestResult<T> { Status = ECustomResultStatus.HasValidation, Validations = validations };
    }

    public new static RequestResult<T> WithValidations(string propertyName, string description)
    {
        return WithValidations(new Validation(propertyName, description));
    }

    public new static RequestResult<T> WithValidations(params ValidationErrorMessage[] validations)
    {
        var result = new RequestResult<T> { Status = ECustomResultStatus.HasValidation };
        result.ValidationResult.AddErrors(validations.Select(v => (v.Message, v.Field)));
        return result;
    }
}