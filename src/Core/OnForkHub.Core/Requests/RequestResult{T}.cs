using OnForkHub.Core.Interfaces.Requests;

namespace OnForkHub.Core.Requests;

public class RequestResult<T> : RequestResult, IRequestCustomResult<T>
{
    public T? Data { get; private init; }

    public new static RequestResult<T> EntityAlreadyExists(string entity, object id, string description)
    {
        return CreateEntityError<T>(entity, id, description, EResultStatus.EntityAlreadyExists);
    }

    public new static RequestResult<T> EntityHasError(string entity, object id, string description)
    {
        return CreateEntityError<T>(entity, id, description, EResultStatus.EntityHasError);
    }

    public new static RequestResult<T> EntityNotFound(string entity, object id, string description)
    {
        return CreateEntityError<T>(entity, id, description, EResultStatus.EntityNotFound);
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
        return new RequestResult<T> { Data = data, Status = EResultStatus.Success };
    }

    public new static RequestResult<T> WithError(string message)
    {
        return new RequestResult<T> { Status = EResultStatus.HasError, RequestError = new RequestError(message) };
    }

    public new static RequestResult<T> WithError(Exception exception)
    {
        return WithError(exception.Message);
    }

    public new static RequestResult<T> WithError(List<string> generalErrors)
    {
        return new RequestResult<T> { Status = EResultStatus.HasError, _generalErrors = generalErrors };
    }

    public new static RequestResult<T> WithError(Dictionary<string, List<string>> entityErrors)
    {
        return new RequestResult<T> { Status = EResultStatus.EntityHasError, _entityErrors = entityErrors };
    }

    public new static RequestResult<T> WithError(RequestError error)
    {
        return new RequestResult<T> { Status = EResultStatus.HasError, RequestError = error };
    }

    public new static RequestResult<T> WithNoContent()
    {
        return new RequestResult<T> { Status = EResultStatus.NoContent };
    }

    public new static RequestResult<T> WithValidationError(string errorMessage, string fieldName = "")
    {
        var result = new RequestResult<T> { Status = EResultStatus.HasValidation };
        result.ValidationResult.AddError(errorMessage, fieldName);
        return result;
    }

    public new static RequestResult<T> WithValidations(params RequestValidation[] validations)
    {
        return new RequestResult<T> { Status = EResultStatus.HasValidation, Validations = validations };
    }

    public new static RequestResult<T> WithValidations(string propertyName, string description)
    {
        return WithValidations(new RequestValidation(propertyName, description));
    }

    public new static RequestResult<T> WithValidations(params ValidationErrorMessage[] validations)
    {
        var result = new RequestResult<T> { Status = EResultStatus.HasValidation };

        foreach (var validation in validations)
        {
            result.ValidationResult.AddError(validation.Message, validation.Field, validation.Source);
        }

        return result;
    }
}
