namespace OnForkHub.Persistence.Exceptions;

public class DatabaseOperationException(string operation, string details)
    : PersistenceException($"Error during operation '{operation}': {details}", "DB_OPERATION_ERROR")
{
    public string Operation { get; } = operation;

    public string Details { get; } = details;
}