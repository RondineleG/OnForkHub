namespace OnForkHub.Persistence.Exceptions;

public static class PersistenceExceptionHandler
{
    public static string EntityNotFound(string entityName, long id)
    {
        return $"{entityName} not found with ID: {id}.";
    }

    public static PersistenceException HandleDbException(Exception exception, string operation, string entityName)
    {
        return exception is DbUpdateException databaseUpdateException
            ? HandleDbUpdateException(databaseUpdateException, operation, entityName)
            : new DatabaseOperationException(operation, exception.Message);
    }

    public static string UnexpectedError(string operation, string message)
    {
        return $"Unexpected error when {operation}: {message}";
    }

    private static string GetAffectedField(string errorMessage)
    {
        try
        {
            if (errorMessage.Contains('\'', StringComparison.Ordinal))
            {
                var start = errorMessage.IndexOf("column '", StringComparison.Ordinal) + 8;
                var end = errorMessage.IndexOf('\'', start);
                return errorMessage[start..end];
            }
        }
        catch (Exception ex)
        {
            return $"unknown field : {ex.Message}";
        }

        return $"unknown field : {errorMessage}";
    }

    private static Exception GetInnermostException(Exception exception)
    {
        while (exception.InnerException != null)
        {
            exception = exception.InnerException;
        }

        return exception;
    }

    private static PersistenceException HandleDbUpdateException(DbUpdateException exception, string operation, string entityName)
    {
        var innerException = GetInnermostException(exception);
        var errorMessage = innerException.Message;

        return errorMessage switch
        {
            var msg when msg.Contains("duplicate key", StringComparison.OrdinalIgnoreCase) => new UniqueConstraintException(entityName),

            var msg when msg.Contains("FOREIGN KEY constraint", StringComparison.OrdinalIgnoreCase) => new ForeignKeyViolationException(
                "The referenced record does not exist"
            ),

            var msg when msg.Contains("REFERENCE constraint", StringComparison.OrdinalIgnoreCase) => new ReferenceConstraintException(
                "This record cannot be deleted because it is being referenced by other records"
            ),

            var msg when msg.Contains("Invalid column name", StringComparison.OrdinalIgnoreCase) => new SchemaErrorException(
                "A specified column does not exist"
            ),

            var msg when msg.Contains("String or binary data would be truncated", StringComparison.OrdinalIgnoreCase) => new DataTruncationException(
                GetAffectedField(errorMessage)
            ),

            var msg when msg.Contains("Cannot insert the value NULL", StringComparison.OrdinalIgnoreCase) => new NullConstraintException(
                GetAffectedField(errorMessage)
            ),

            _ => new DatabaseOperationException(operation, errorMessage),
        };
    }
}
