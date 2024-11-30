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

    public static async Task EnsureGitFlowConfiguredAsync()
    {
        try
        {
            if (!await VerifyGitInstallationAsync())
            {
                throw new InvalidOperationException("Git is not installed or not accessible.");
            }

            await EnsureCleanWorkingTreeAsync();

            var currentBranch = await GetCurrentBranchAsync();

            await RunProcessAsync("git", "config gitflow.branch.master main");
            await RunProcessAsync("git", "config gitflow.branch.develop dev");
            await RunProcessAsync("git", "config gitflow.prefix.feature feature/");
            await RunProcessAsync("git", "config gitflow.prefix.release release/");
            await RunProcessAsync("git", "config gitflow.prefix.hotfix hotfix/");
            await RunProcessAsync("git", "config gitflow.prefix.support support/");
            await RunProcessAsync("git", "config gitflow.prefix.versiontag v");

            await EnsureBranchExistsAsync("main");
            await EnsureBranchExistsAsync("dev");

            try
            {
                await RunProcessAsync("git", "flow init -f -d");
                Console.WriteLine("[INFO] Git Flow configured successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Initial Git Flow initialization failed, but configuration should still work: {ex.Message}");
            }

            if (!string.IsNullOrEmpty(currentBranch))
            {
                Console.WriteLine($"[INFO] Returning to the original branch: {currentBranch}...");
                await RunProcessAsync("git", $"checkout {currentBranch}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("[ERROR] An error occurred while configuring Git Flow:");
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
                try
                {
                    Console.WriteLine($"[INFO] Checking remote for branch '{branchName}'...");
                    await RunProcessAsync("git", "fetch origin");
                    var remoteOutput = await RunProcessAsync("git", $"ls-remote --heads origin {branchName}");

                    if (!string.IsNullOrWhiteSpace(remoteOutput))
                    {
                        Console.WriteLine($"[INFO] Branch '{branchName}' found in remote. Creating local branch...");
                        await RunProcessAsync("git", $"checkout -b {branchName} origin/{branchName}");
                    }
                    else
                    {
                        Console.WriteLine($"[INFO] Branch '{branchName}' not found in remote. Creating new branch...");
                        var defaultBranch = await GetDefaultBranchAsync();
                        await RunProcessAsync("git", $"checkout -b {branchName} {defaultBranch}");
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine($"[INFO] Creating new local branch '{branchName}'...");
                    var defaultBranch = await GetDefaultBranchAsync();
                    await RunProcessAsync("git", $"checkout -b {branchName} {defaultBranch}");
                }
                Console.WriteLine($"[INFO] Branch '{branchName}' created successfully.");
            }
            else
            {
                Console.WriteLine($"[INFO] Branch '{branchName}' already exists locally.");
                await RunProcessAsync("git", $"checkout {branchName}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to ensure branch '{branchName}' exists:");
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

    private static async Task<string> GetDefaultBranchAsync()
    {
        try
        {
            var result = await RunProcessAsync("git", "rev-parse --abbrev-ref origin/HEAD");
            return result.Replace("origin/", "").Trim();
        }
        catch
        {
            return "main";
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
