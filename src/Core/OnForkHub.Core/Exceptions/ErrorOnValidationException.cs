namespace OnForkHub.Core.Exceptions;

public class ErrorOnValidationException(string message, string errorCode = "VALIDATION_ERROR") : CustomException(message, errorCode) { }