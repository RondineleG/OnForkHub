// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Persistence.Exceptions;

public class DataTruncationException(string field)
    : PersistenceException($"The data provided for {field} is too long for the database field.", "DATA_TRUNCATION_ERROR") { }
