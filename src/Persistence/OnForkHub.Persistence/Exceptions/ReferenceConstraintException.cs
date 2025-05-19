// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public class ReferenceConstraintException(string message) : PersistenceException(message, "REFERENCE_CONSTRAINT_ERROR") { }
