namespace OnForkHub.Scripts.Interfaces;

public interface IProcessRunner
{
    Task<string> RunAsync(string fileName, string arguments, string? workingDirectory = null);
}