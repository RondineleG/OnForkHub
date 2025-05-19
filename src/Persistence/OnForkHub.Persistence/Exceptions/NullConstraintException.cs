// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public class NullConstraintException(string field)
    : PersistenceException($"Cannot insert null value into required field: {field}", "NULL_CONSTRAINT_ERROR") { }
