namespace OnForkHub.Core.Exceptions;

public class NotFoundException(string message, string errorCode = "NOT_FOUND") : CustomException(message, errorCode)
{
}