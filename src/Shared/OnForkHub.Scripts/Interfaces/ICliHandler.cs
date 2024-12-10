namespace OnForkHub.Scripts.Interfaces;

public interface ICliHandler
{
    void ShowHelp();
    Task<bool> HandlePackageCommand(string[] args);
}