// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Exceptions;

public class NotFoundException(string message, string errorCode = "NOT_FOUND") : CustomException(message, errorCode) { }
