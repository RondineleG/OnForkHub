// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Exceptions;

public class ConflictException(string message, string errorCode = "CONFLICT_ERROR") : CustomException(message, errorCode) { }
