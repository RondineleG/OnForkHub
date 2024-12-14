namespace OnForkHub.Scripts;

public class ProcessRunner(ILogger logger) : IProcessRunner
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<string> RunAsync(string fileName, string arguments, string? workingDirectory = null)
    {
        _logger.Log(ELogLevel.Info, $"Executing command: {fileName} {arguments}");

        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            processInfo.WorkingDirectory = workingDirectory;
        }

        try
        {
            using var process = Process.Start(processInfo) ?? throw new GitOperationException("Failed to start process.");

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new GitOperationException($"Command failed with exit code {process.ExitCode}. {error}");
            }

            _logger.Log(ELogLevel.Info, "Command executed successfully.");
            return output;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to execute command: {fileName} {arguments}");
            throw new GitOperationException($"Command execution failed: {ex.Message}", ex);
        }
    }
}