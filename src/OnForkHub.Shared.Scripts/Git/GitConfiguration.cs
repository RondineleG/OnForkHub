namespace OnForkHub.Shared.Scripts.Git;

public sealed class GitConfiguration
{
    public async Task<bool> VerifyGitInstallationAsync()
    {
        try
        {
            var gitVersion = await RunGitCommandAsync("--version");
            Console.WriteLine($"Git version: {gitVersion.Trim()}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error verifying Git installation:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public async Task ApplySharedConfigurationsAsync()
    {
        Console.WriteLine("Applying shared Git configurations...");

        var existingValuesFound = false;

        try
        {
            Console.WriteLine("Checking for existing 'include.path' values...");

            try
            {
                var existingValues = await RunGitCommandAsync("config --local --get-all include.path");
                if (!string.IsNullOrWhiteSpace(existingValues))
                {
                    Console.WriteLine("Found existing 'include.path' values:");
                    Console.WriteLine(existingValues);
                    existingValuesFound = true;
                }
            }
            catch (InvalidOperationException ex)
            {
                if (ex.Message.Contains("exit code 1"))
                {
                    Console.WriteLine("No existing 'include.path' values found.");
                }
                else
                {
                    throw;
                }
            }

            await RunGitCommandAsync("config --local --replace-all include.path ../.gitconfig");
            Console.WriteLine("Configuration applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error applying Git configuration:");
            Console.WriteLine(ex.Message);

            if (!existingValuesFound)
            {
                Console.WriteLine("No changes were made to the Git configuration.");
            }
            else
            {
                Console.WriteLine("The configuration attempt failed after finding existing values.");
            }
        }
    }

    private async Task<string> RunGitCommandAsync(string command)
    {
        var processInfo = new ProcessStartInfo("git", command)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(processInfo);
        if (process == null)
        {
            throw new InvalidOperationException("Failed to start Git process.");
        }

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Git command failed with exit code {process.ExitCode}. {error}");
        }

        return output;
    }
}
