namespace OnForkHub.Core.Exceptions;

public class ConflictException(string message, string errorCode = "CONFLICT_ERROR") : CustomException(message, errorCode) { }
