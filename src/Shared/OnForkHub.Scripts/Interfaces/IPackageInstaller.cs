namespace OnForkHub.Scripts.Interfaces;

public interface IPackageInstaller
{
    Task InstallPackageDirectAsync(string packageId, string version = "");
    Task SearchAndInstallInteractiveAsync(string? searchTerm = null);
}