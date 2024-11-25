using System.Globalization;

namespace OnForkHub.Scripts.Git;

public partial class PullRequestConfiguration
{
    [GeneratedRegex(@"(feature/[^:\s]+|hotfix/[^:\s]+|bugfix/[^:\s]+|release/[^:\s]+)")]
    public static partial Regex BranchNameRegex();

    public static async Task CreatePullRequestForGitFlowFinishAsync()
    {
        var isFinishCommand = await IsGitFlowFinishCommandAsync();
        if (!isFinishCommand)
        {
            Console.WriteLine("[DEBUG] Not a finish command, skipping PR creation");
            return;
        }
        Console.WriteLine("[DEBUG] Starting CreatePullRequestForGitFlowFinishAsync");
        try
        {
            var currentBranch = await RunProcessAsync("git", "branch --show-current");
            Console.WriteLine($"[DEBUG] Current branch output: {currentBranch}");

            var match = BranchNameRegex().Match(currentBranch);
            Console.WriteLine($"[DEBUG] Regex match success: {match.Success}");
            if (!match.Success)
            {
                Console.WriteLine("[INFO] No matching branch found");
                return;
            }

            var sourceBranch = match.Groups[1].Value;
            Console.WriteLine($"[INFO] Source branch: {sourceBranch}");

            var hasCommits = await HasCommitsAsync(sourceBranch);
            if (!hasCommits)
            {
                Console.WriteLine("[INFO] No commits found in branch, skipping PR creation");
                return;
            }

            var prInfo = await GetPullRequestInfoAsync(sourceBranch);
            Console.WriteLine($"[DEBUG] PR Info created: {(prInfo != null ? "yes" : "no")}");
            if (prInfo is null)
            {
                Console.WriteLine("[INFO] Branch type not recognized");
                return;
            }

            Console.WriteLine("[DEBUG] Attempting to authenticate with GitHub CLI");
            await AuthenticateWithGitHubCliAsync();

            await RunProcessAsync("git", $"push origin {sourceBranch}");

            Console.WriteLine("[DEBUG] Creating pull request");
            await CreatePullRequestWithGitHubCLIAsync(prInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create pull request: {ex.Message}");
            Console.WriteLine($"[ERROR] Stack trace: {ex.StackTrace}");
            throw;
        }
    }

    private static async Task<bool> IsGitFlowFinishCommandAsync()
    {
        try
        {
            var gitDir = await RunProcessAsync("git", "rev-parse --git-dir");
            var originalArgs = await File.ReadAllTextAsync(Path.Combine(gitDir.Trim(), "gitflow.finish"));
            Console.WriteLine($"[DEBUG] 1207 {originalArgs}");
            return !string.IsNullOrEmpty(originalArgs);
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> HasCommitsAsync(string branch)
    {
        try
        {
            var result = await RunProcessAsync("git", $"rev-list --count dev..{branch}");
            Console.WriteLine($"[DEBUG] 1207 {result}");
            return int.Parse(result.Trim(), CultureInfo.InvariantCulture) > 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task CreatePullRequestWithGitHubCLIAsync(PullRequestInfo prInfo)
    {
        try
        {
            var command = $"pr create --title \"{prInfo.Title}\" --body \"{prInfo.Body}\" --base {prInfo.BaseBranch} --head {prInfo.SourceBranch}";
            Console.WriteLine($"[DEBUG] Creating PR with command: gh {command}");
            var result = await RunProcessAsync("gh", command);
            Console.WriteLine($"[INFO] Successfully created PR: {result}");

            if (
                prInfo.SourceBranch.StartsWith("hotfix/", StringComparison.Ordinal)
                || prInfo.SourceBranch.StartsWith("release/", StringComparison.Ordinal)
            )
            {
                command = $"pr create --title \"{prInfo.Title}\" --body \"{prInfo.Body}\" --base dev --head {prInfo.SourceBranch}";
                Console.WriteLine($"[DEBUG] Creating additional PR with command: gh {command}");
                result = await RunProcessAsync("gh", command);
                Console.WriteLine($"[INFO] Successfully created additional PR to dev: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create PR with GitHub CLI: {ex.Message}");
            throw;
        }
    }

    private static async Task AuthenticateWithGitHubCliAsync()
    {
        Console.WriteLine("[DEBUG] Starting GitHub CLI authentication check");
        try
        {
            if (!await IsGitHubCliInstalledAsync())
            {
                throw new InvalidOperationException("GitHub CLI (gh) is not installed");
            }

            var status = await RunProcessAsync("gh", "auth status");
            Console.WriteLine($"[DEBUG] GitHub CLI auth status: {status}");

            if (string.IsNullOrWhiteSpace(status))
            {
                Console.WriteLine("[DEBUG] Attempting GitHub CLI login");
                var loginResult = await RunProcessAsync("gh", "auth login");
                if (string.IsNullOrWhiteSpace(loginResult))
                {
                    throw new InvalidOperationException("Failed to authenticate with GitHub CLI");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] GitHub CLI authentication error: {ex.Message}");
            throw;
        }
    }

    private static async Task<string> RunProcessAsync(string fileName, string arguments)
    {
        Console.WriteLine($"[DEBUG] Running process: {fileName} {arguments}");
        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = Process.Start(processInfo) ?? throw new InvalidOperationException("Failed to start process");
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0 && !string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"[ERROR] Process failed with exit code {process.ExitCode}");
            Console.WriteLine($"[ERROR] Error output: {error}");
            throw new InvalidOperationException($"Command failed: {error}");
        }

        Console.WriteLine($"[DEBUG] Process output: {output}");
        return output;
    }

    private static async Task<bool> IsGitHubCliInstalledAsync()
    {
        var result = await RunProcessAsync("gh", "--version");
        return !string.IsNullOrWhiteSpace(result);
    }

    private static Task<PullRequestInfo?> GetPullRequestInfoAsync(string branchName)
    {
        try
        {
            if (string.IsNullOrEmpty(branchName))
            {
                return Task.FromResult<PullRequestInfo?>(null);
            }

            var info = new PullRequestInfo(branchName);

            if (branchName.StartsWith("feature/", StringComparison.Ordinal))
            {
                info.Title = $"Feature completed: {branchName.Replace("feature/", "", StringComparison.Ordinal)}";
                info.Body = "This feature is ready for review.";
                info.BaseBranch = "dev";
            }
            else if (branchName.StartsWith("hotfix/", StringComparison.Ordinal))
            {
                info.Title = $"Hotfix: {branchName.Replace("hotfix/", "", StringComparison.Ordinal)}";
                info.Body = "Urgent hotfix for production.";
                info.BaseBranch = "main";
            }
            else if (branchName.StartsWith("bugfix/", StringComparison.Ordinal))
            {
                info.Title = $"Bugfix: {branchName.Replace("bugfix/", "", StringComparison.Ordinal)}";
                info.Body = "Bug fixed and ready for review.";
                info.BaseBranch = "dev";
            }
            else if (branchName.StartsWith("release/", StringComparison.Ordinal))
            {
                info.Title = $"Release: {branchName.Replace("release/", "", StringComparison.Ordinal)}";
                info.Body = "Release is ready for production.";
                info.BaseBranch = "main";
            }
            else
            {
                return Task.FromResult<PullRequestInfo?>(null);
            }

            return Task.FromResult<PullRequestInfo?>(info);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error preparing PR info: {ex.Message}");
            return Task.FromResult<PullRequestInfo?>(null);
        }
    }
}
