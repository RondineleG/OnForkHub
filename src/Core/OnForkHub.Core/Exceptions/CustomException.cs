namespace OnForkHub.Core.Exceptions;

public abstract class CustomException(string message, string errorCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
}
