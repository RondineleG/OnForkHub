namespace OnForkHub.Persistence.Exceptions;

public class ReferenceConstraintException(string message) : PersistenceException(message, "REFERENCE_CONSTRAINT_ERROR") { }
