// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public class SchemaErrorException(string details) : PersistenceException($"Database schema error: {details}", "SCHEMA_ERROR") { }
