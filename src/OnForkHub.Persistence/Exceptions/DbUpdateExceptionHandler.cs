namespace OnForkHub.Persistence.Exceptions;

public static class DbUpdateExceptionHandler
{
    public static string HandleDbUpdateException(DbUpdateException exception)
    {
        var innerException = exception.InnerException;
        while (innerException?.InnerException != null)
        {
            innerException = innerException.InnerException;
        }

        var errorMessage = innerException?.Message ?? exception.Message;

        if (errorMessage.Contains("duplicate key"))
        {
            return "Unique constraint violation. A record with the same unique data already exists.";
        }
        else if (errorMessage.Contains("The INSERT statement conflicted with the FOREIGN KEY constraint"))
        {
            return "Foreign key violation. The referenced record does not exist.";
        }
        else if (errorMessage.Contains("The DELETE statement conflicted with the REFERENCE constraint"))
        {
            return "This record cannot be deleted because it is being referenced by other records.";
        }
        else if (errorMessage.Contains("Invalid column name"))
        {
            return "Database schema error. A specified column does not exist.";
        }
        else if (errorMessage.Contains("String or binary data would be truncated"))
        {
            return "The data provided is too long for the corresponding field in the database.";
        }
        else if (errorMessage.Contains("Cannot insert the value NULL into column"))
        {
            return "Attempting to insert a null value into a field that does not allow nulls.";
        }

        return $"Error updating database: {errorMessage}";
    }
}
