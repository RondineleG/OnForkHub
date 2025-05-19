// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public class ForeignKeyViolationException(string details) : PersistenceException($"Foreign key violation: {details}", "FOREIGN_KEY_ERROR") { }
