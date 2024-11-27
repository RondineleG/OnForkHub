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

    public static async Task EnsureCleanWorkingTreeAsync()
    {
        try
        {
            Console.WriteLine("[INFO] Checking for unstaged changes...");
            var statusOutput = await RunProcessAsync("git", "status --porcelain");
            if (!string.IsNullOrWhiteSpace(statusOutput))
            {
                Console.WriteLine("[ERROR] Working tree contains unstaged changes. Please commit or stash changes before proceeding.");
                throw new InvalidOperationException("Working tree is not clean.");
            }
            Console.WriteLine("[INFO] Working tree is clean.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to verify clean working tree:");
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static async Task EnsureBranchExistsAsync(string branchName)
    {
        try
        {
            Console.WriteLine($"[INFO] Checking if branch '{branchName}' exists...");
            var branchesOutput = await RunProcessAsync("git", "branch --list");
            if (!branchesOutput.Contains(branchName))
            {
                Console.WriteLine($"[INFO] Branch '{branchName}' does not exist. Checking out from origin/{branchName}...");
                await RunProcessAsync("git", $"checkout -b {branchName} origin/{branchName}");
                Console.WriteLine($"[INFO] Branch '{branchName}' created successfully from origin.");
            }
            else
            {
                Console.WriteLine($"[INFO] Branch '{branchName}' already exists locally.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to ensure branch '{branchName}' exists:");
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public static async Task EnsureGitFlowConfiguredAsync()
    {
        try
        {
            Console.WriteLine("[INFO] Checking if Git Flow is already configured...");
            await RunProcessAsync("git", "flow config");
            Console.WriteLine("[INFO] Git Flow is already configured.");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Not a gitflow-enabled repo"))
        {
            Console.WriteLine("[INFO] Git Flow is not configured. Initializing...");

            Console.WriteLine("[INFO] Saving current branch...");
            var currentBranch = await GetCurrentBranchAsync();

            await EnsureBranchExistsAsync("main");
            await EnsureBranchExistsAsync("dev");

            Console.WriteLine($"[INFO] Returning to the original branch: {currentBranch}...");
            await RunProcessAsync("git", $"checkout {currentBranch}");

            await RunProcessAsync("git", "flow init -d");
            Console.WriteLine("[INFO] Git Flow configured successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] An error occurred while checking Git Flow configuration:");
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    private static async Task<string> GetCurrentBranchAsync()
    {
        try
        {
            Console.WriteLine("[INFO] Retrieving current branch...");
            var branchOutput = await RunProcessAsync("git", "rev-parse --abbrev-ref HEAD");
            Console.WriteLine($"[INFO] Current branch: {branchOutput.Trim()}");
            return branchOutput.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] Failed to retrieve current branch:");
            Console.WriteLine(ex.Message);
            throw;
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
