// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Exceptions;

public class CustomResultException(RequestResult customResult) : Exception(customResult?.Message ?? "An error occurred")
{
    public CustomResultException(params RequestValidation[] validations)
        : this(RequestResult.WithValidations(validations)) { }

    public CustomResultException(Exception exception)
        : this(RequestResult.WithError(exception)) { }

    public RequestResult CustomResult { get; } = customResult;
}
