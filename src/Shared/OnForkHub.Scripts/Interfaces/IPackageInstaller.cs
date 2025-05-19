// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Interfaces;

public interface IPackageInstaller
{
    Task InstallPackageDirectAsync(string packageId, string version = "");

    Task SearchAndInstallInteractiveAsync(string? searchTerm = null);
}
