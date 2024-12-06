using OnForkHub.Core.Interfaces.Requests;

namespace OnForkHub.Core.Requests;

public class RequestResult : IRequestValidations, IRequestError, IRequestEntityWarning
{
    internal Dictionary<string, List<string>>? _entityErrors;

    internal List<string>? _generalErrors;

    public RequestResult()
    {
        Status = EResultStatus.Success;
        ValidationResult = new ValidationResult();
    }

    public DateTime Date { get; set; } = DateTime.Now;

    public Dictionary<string, List<string>> EntityErrors => _entityErrors ??= [];

    public List<string> GeneralErrors => _generalErrors ??= [];

    public string Id { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public RequestEntityWarning? RequestEntityWarning { get; protected init; }

    public RequestError? RequestError { get; protected init; }

    public EResultStatus Status { get; set; }

    public ValidationResult ValidationResult { get; protected set; }

    public IEnumerable<RequestValidation> Validations { get; protected init; } = [];

    public static RequestResult EntityAlreadyExists(string entity, object id, string description)
    {
        return CreateEntityError(entity, id, description, EResultStatus.EntityAlreadyExists);
    }

    public static RequestResult EntityHasError(string entity, object id, string description)
    {
        return CreateEntityError(entity, id, description, EResultStatus.EntityHasError);
    }

    public static RequestResult EntityNotFound(string entity, object id, string description)
    {
        return CreateEntityError(entity, id, description, EResultStatus.EntityNotFound);
    }

    public static RequestResult Success()
    {
        return new RequestResult { Status = EResultStatus.Success };
    }

    public static RequestResult WithError(string message)
    {
        return new RequestResult { Status = EResultStatus.HasError, RequestError = new RequestError(message) };
    }

    public static RequestResult WithError(Exception exception)
    {
        return WithError(exception.Message);
    }

    public static RequestResult WithError(List<string> generalErrors)
    {
        return new RequestResult { Status = EResultStatus.HasError, _generalErrors = generalErrors };
    }

    public static RequestResult WithError(Dictionary<string, List<string>> entityErrors)
    {
        return new RequestResult { Status = EResultStatus.EntityHasError, _entityErrors = entityErrors };
    }

    public static RequestResult WithError(RequestError error)
    {
        return new RequestResult { Status = EResultStatus.HasError, RequestError = error };
    }

    public static RequestResult WithNoContent()
    {
        return new RequestResult { Status = EResultStatus.NoContent };
    }

    public static RequestResult WithValidationError(string errorMessage, string fieldName = "")
    {
        var result = new RequestResult { Status = EResultStatus.HasValidation };
        result.ValidationResult.AddError(errorMessage, fieldName);
        return result;
    }

    public static RequestResult WithValidations(params RequestValidation[] validations)
    {
        var result = new RequestResult { Status = EResultStatus.HasValidation };

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
        var result = new RequestResult { Status = EResultStatus.HasValidation };

        foreach (var validation in validations)
        {
            result.ValidationResult.AddError(validation.Message, validation.Field, validation.Source);
        }

        return result;
    }

    public void AddEntityError(string entity, string message)
    {
        Status = EResultStatus.EntityHasError;
        if (!EntityErrors.TryGetValue(entity, out var value))
        {
            value = [];
            EntityErrors[entity] = value;
        }

        value.Add(message);
    }

    public void AddError(string message)
    {
        Status = EResultStatus.HasError;
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

        if ((ValidationResult?.Errors != null) && (ValidationResult.Errors.Count != 0))
        {
            foreach (var validationError in ValidationResult.Errors)
            {
                messages.Add($"{validationError.Field}: {validationError.Message}");
            }
        }

        return string.Join("; ", messages);
    }

    protected static RequestResult CreateEntityError(string entity, object id, string description, EResultStatus status)
    {
        return new RequestResult { Status = status, RequestEntityWarning = new RequestEntityWarning(entity, id, description) };
    }

    protected static RequestResult<T> CreateEntityError<T>(string entity, object id, string description, EResultStatus status)
    {
        return new RequestResult<T> { Status = status, RequestEntityWarning = new RequestEntityWarning(entity, id, description) };
    }
}

public class RequestResult<T> : RequestResult, IRequestCustomResult<T>
{
    public T? Data { get; private init; }

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

    public static new RequestResult<T> EntityAlreadyExists(string entity, object id, string description)
    {
        return CreateEntityError<T>(entity, id, description, EResultStatus.EntityAlreadyExists);
    }

    public static new RequestResult<T> EntityHasError(string entity, object id, string description)
    {
        return CreateEntityError<T>(entity, id, description, EResultStatus.EntityHasError);
    }

    public static new RequestResult<T> EntityNotFound(string entity, object id, string description)
    {
        return CreateEntityError<T>(entity, id, description, EResultStatus.EntityNotFound);
    }

    public static RequestResult<T> Success(T data)
    {
        return new RequestResult<T> { Data = data, Status = EResultStatus.Success };
    }

    public static new RequestResult<T> WithError(string message)
    {
        return new RequestResult<T> { Status = EResultStatus.HasError, RequestError = new RequestError(message) };
    }

    public static new RequestResult<T> WithError(Exception exception)
    {
        return WithError(exception.Message);
    }

    public static new RequestResult<T> WithError(List<string> generalErrors)
    {
        return new RequestResult<T> { Status = EResultStatus.HasError, _generalErrors = generalErrors };
    }

    public static new RequestResult<T> WithError(Dictionary<string, List<string>> entityErrors)
    {
        return new RequestResult<T> { Status = EResultStatus.EntityHasError, _entityErrors = entityErrors };
    }

    public static new RequestResult<T> WithError(RequestError error)
    {
        return new RequestResult<T> { Status = EResultStatus.HasError, RequestError = error };
    }

    public static new RequestResult<T> WithNoContent()
    {
        return new RequestResult<T> { Status = EResultStatus.NoContent };
    }

    public static new RequestResult<T> WithValidationError(string errorMessage, string fieldName = "")
    {
        var result = new RequestResult<T> { Status = EResultStatus.HasValidation };
        result.ValidationResult.AddError(errorMessage, fieldName);
        return result;
    }

    public static new RequestResult<T> WithValidations(params RequestValidation[] validations)
    {
        return new RequestResult<T> { Status = EResultStatus.HasValidation, Validations = validations };
    }

    public static new RequestResult<T> WithValidations(string propertyName, string description)
    {
        return WithValidations(new RequestValidation(propertyName, description));
    }

    public static new RequestResult<T> WithValidations(params ValidationErrorMessage[] validations)
    {
        var result = new RequestResult<T> { Status = EResultStatus.HasValidation };

        foreach (var validation in validations)
        {
            result.ValidationResult.AddError(validation.Message, validation.Field, validation.Source);
        }

        return result;
    }
}
