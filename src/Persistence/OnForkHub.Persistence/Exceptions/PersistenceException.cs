// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public abstract class PersistenceException(string message, string errorCode = "PERSISTENCE_ERROR") : CustomException(message, errorCode) { }
