namespace OnForkHub.Core.Exceptions;

public class CustomResultException(RequestResult customResult) : Exception((customResult?.Message) ?? "An error occurred")
{
    public RequestResult CustomResult { get; } = customResult;

    public CustomResultException(params RequestValidation[] validations)
        : this(RequestResult.WithValidations(validations)) { }

    public CustomResultException(Exception exception)
        : this(RequestResult.WithError(exception)) { }
}
