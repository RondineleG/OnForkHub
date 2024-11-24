using System.Text.RegularExpressions;

namespace OnForkHub.Scripts.Git;

public partial class PullRequestManager
{
    public static async Task CreatePullRequestForGitFlowFinishAsync()
    {
        try
        {
            var currentBranch = await GetCurrentBranchAsync();
            Console.WriteLine($"[INFO] Current branch: {currentBranch}");

            var previousBranch = await GetPreviousBranchAsync();
            if (string.IsNullOrEmpty(previousBranch))
            {
                Console.WriteLine("[INFO] No previous branch found. Skipping PR creation.");
                return;
            }

            Console.WriteLine($"[INFO] Previous branch: {previousBranch}");

            var prInfo = GetPullRequestInfo(previousBranch);
            if (prInfo == null)
            {
                Console.WriteLine("[INFO] Branch type not recognized. Skipping PR creation.");
                return;
            }

            await PushBranchAsync(previousBranch);
            await CreatePullRequestAsync(prInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create pull request: {ex.Message}");
        }
    }

    private static async Task<string> GetCurrentBranchAsync()
    {
        return (await RunProcessAsync("git", "rev-parse --abbrev-ref HEAD")).Trim();
    }

    [GeneratedRegex(@"from (feature/[^:\s]+|hotfix/[^:\s]+|bugfix/[^:\s]+|release/[^:\s]+)", RegexOptions.Compiled)]
    private static partial Regex GetBranchNameRegex();

    private static async Task<string> GetPreviousBranchAsync()
    {
        var reflog = await RunProcessAsync("git", "reflog -1");
        var match = GetBranchNameRegex().Match(reflog);
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static PullRequestInfo? GetPullRequestInfo(string branchName)
    {
        if (string.IsNullOrEmpty(branchName))
        {
            return null;
        }

        var info = new PullRequestInfo { SourceBranch = branchName };

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
            return null;
        }

        return info;
    }

    private static async Task PushBranchAsync(string branch)
    {
        try
        {
            await RunProcessAsync("git", $"push origin {branch}");
            Console.WriteLine($"[INFO] Successfully pushed branch {branch}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARN] Failed to push branch: {ex.Message}");
        }
    }

    private static async Task CreatePullRequestAsync(PullRequestInfo prInfo)
    {
        try
        {
            var command = $"pr create --title \"{prInfo.Title}\" --body \"{prInfo.Body}\" --base {prInfo.BaseBranch} --head {prInfo.SourceBranch}";
            var result = await RunProcessAsync("gh", command);
            Console.WriteLine($"[INFO] Successfully created PR: {result}");

            if (
                prInfo.SourceBranch.StartsWith("hotfix/", StringComparison.Ordinal)
                || prInfo.SourceBranch.StartsWith("release/", StringComparison.Ordinal)
            )
            {
                command = $"pr create --title \"{prInfo.Title}\" --body \"{prInfo.Body}\" --base dev --head {prInfo.SourceBranch}";
                result = await RunProcessAsync("gh", command);
                Console.WriteLine($"[INFO] Successfully created additional PR to dev: {result}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create PR: {ex.Message}");
        }
    }

    private static async Task<string> RunProcessAsync(string fileName, string arguments)
    {
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

        return process.ExitCode != 0 && !string.IsNullOrEmpty(error) ? throw new InvalidOperationException($"Command failed: {error}") : output;
    }
}
