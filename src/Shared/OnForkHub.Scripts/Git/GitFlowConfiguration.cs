namespace OnForkHub.Scripts.Git;

public static class GitFlowConfiguration
{
    public static async Task<bool> VerifyGitInstallationAsync()
    {
        try
        {
            Console.WriteLine("[INFO] Checking Git installation...");
            var gitVersion = await RunProcessAsync("git", "--version");
            Console.WriteLine($"[INFO] Git Version: {gitVersion.Trim()}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to verify Git installation:");
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public static async Task ApplySharedConfigurationsAsync()
    {
        Console.WriteLine("[INFO] Applying shared Git configurations...");
        var existingValuesFound = false;

        try
        {
            Console.WriteLine("[INFO] Checking existing values for 'include.path'...");

            try
            {
                var existingValues = await RunProcessAsync("git", "config --local --get-all include.path");
                if (!string.IsNullOrWhiteSpace(existingValues))
                {
                    Console.WriteLine("[INFO] Existing values found:");
                    Console.WriteLine(existingValues);
                    existingValuesFound = true;
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("exit code 1"))
            {
                Console.WriteLine("[INFO] No existing values found for 'include.path'.");
            }

            Console.WriteLine("[INFO] Replacing existing values with '../.gitconfig'...");
            await RunProcessAsync("git", "config --local --replace-all include.path ../.gitconfig");
            Console.WriteLine("[INFO] Configuration applied successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to apply Git configurations:");
            Console.WriteLine(ex.Message);

            if (!existingValuesFound)
            {
                Console.WriteLine("[INFO] No changes were made to Git configuration.");
            }
            else
            {
                Console.WriteLine("[INFO] Configuration attempt failed after finding existing values.");
            }
        }
    }

    private static async Task<string> RunProcessAsync(string fileName, string arguments)
    {
        Console.WriteLine($"[INFO] Executing command: {fileName} {arguments}");

        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        try
        {
            using var process = Process.Start(processInfo) ?? throw new InvalidOperationException("[ERROR] Failed to start process.");
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                throw new InvalidOperationException($"[ERROR] Command failed with exit code {process.ExitCode}. {error}");
            }

            Console.WriteLine("[INFO] Command executed successfully.");
            return output;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to execute command: {fileName} {arguments}");
            throw new InvalidOperationException($"[DEBUG] Exception: {ex.Message}", ex);
        }
    }
}
