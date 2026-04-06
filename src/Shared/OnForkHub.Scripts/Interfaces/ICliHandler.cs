namespace OnForkHub.Scripts.Interfaces;

public interface ICliHandler
{
    Task<bool> HandlePackageCommand(string[] args);

    void ShowHelp();
}
