// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public class UniqueConstraintException(string entityName)
    : PersistenceException($"A {entityName} with the same unique data already exists.", "UNIQUE_CONSTRAINT_ERROR") { }
