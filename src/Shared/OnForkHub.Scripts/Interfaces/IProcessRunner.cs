// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Interfaces;

public interface IProcessRunner
{
    Task<string> RunAsync(string fileName, string arguments, string? workingDirectory = null);
}
