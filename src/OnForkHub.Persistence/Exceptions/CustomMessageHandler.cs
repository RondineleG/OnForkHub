namespace OnForkHub.Persistence.Exceptions;

public static class CustomMessageHandler
{
    public static string EntityNotFound(string entityName, long id)
    {
        return $"{entityName} not found with ID: {id}.";
    }

    public static string UnexpectedError(string operation, string message)
    {
        return $"Unexpected error when {operation}: {message}";
    }

    public static string DbUpdateError(DbUpdateException exception)
    {
        return DbUpdateExceptionHandler.HandleDbUpdateException(exception);
    }
}
