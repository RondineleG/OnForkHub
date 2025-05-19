// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Exceptions;

public class ErrorOnValidationException(string message, string errorCode = "VALIDATION_ERROR") : CustomException(message, errorCode) { }
