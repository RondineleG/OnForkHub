// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Git;

public class GitOperationException : Exception
{
    public GitOperationException(string message)
        : base(message) { }

    public GitOperationException(string message, Exception innerException)
        : base(message, innerException) { }
}
