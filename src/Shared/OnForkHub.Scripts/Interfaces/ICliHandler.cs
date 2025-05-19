// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Interfaces;

public interface ICliHandler
{
    Task<bool> HandlePackageCommand(string[] args);

    void ShowHelp();
}
