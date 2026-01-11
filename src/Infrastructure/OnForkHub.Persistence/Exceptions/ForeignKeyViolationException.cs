namespace OnForkHub.Persistence.Exceptions;

public class ForeignKeyViolationException(string details) : PersistenceException($"Foreign key violation: {details}", "FOREIGN_KEY_ERROR") { }
