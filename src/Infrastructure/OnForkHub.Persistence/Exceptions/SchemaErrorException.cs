namespace OnForkHub.Persistence.Exceptions;

public class SchemaErrorException(string details) : PersistenceException($"Database schema error: {details}", "SCHEMA_ERROR") { }
