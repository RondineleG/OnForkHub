namespace OnForkHub.Persistence.Exceptions;

public abstract class PersistenceException(string message, string errorCode = "PERSISTENCE_ERROR") : CustomException(message, errorCode) { }
