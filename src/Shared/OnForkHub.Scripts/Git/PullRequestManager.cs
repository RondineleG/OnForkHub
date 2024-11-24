using System.Text.RegularExpressions;

namespace OnForkHub.Scripts.Git;

public partial class PullRequestManager
{
    public static async Task CreatePullRequestForGitFlowFinishAsync()
    {
        try
        {
            var lastBranch = await GetLastBranchAsync();
            Console.WriteLine($"[INFO] Last branch before merge: {lastBranch}");

            if (string.IsNullOrEmpty(lastBranch))
            {
                Console.WriteLine("[ERROR] Could not determine source branch.");
                return;
            }

            var prInfo = GetPullRequestInfo(lastBranch);
            if (prInfo == null)
            {
                Console.WriteLine("[INFO] Branch type not recognized. Skipping PR creation.");
                return;
            }

            await CreatePullRequestAsync(prInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create pull request: {ex.Message}");
        }
    }

    [GeneratedRegex(@"from (feature/[^:\s]+|hotfix/[^:\s]+|bugfix/[^:\s]+|release/[^:\s]+)", RegexOptions.Compiled)]
    private static partial Regex GetBranchNameRegex();

    private static async Task<string> GetLastBranchAsync()
    {
        try
        {
            var reflog = await RunProcessAsync("git", "reflog -1");
            var match = GetBranchNameRegex().Match(reflog);
            if (match.Success)
            {
                return match.Groups[1].Value;
            }

            var lastCommitMsg = await RunProcessAsync("git", "log -1 --pretty=%B");
            if (lastCommitMsg.Contains("Merge branch", StringComparison.Ordinal))
            {
                match = GetBranchNameRegex().Match(lastCommitMsg);
                if (match.Success)
                {
                    return match.Groups[1].Value;
                }
            }

            var mergedBranches = await RunProcessAsync("git", "branch --merged");
            foreach (var line in mergedBranches.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                var branch = line.Trim();
                if (
                    branch.StartsWith("feature/", StringComparison.Ordinal)
                    || branch.StartsWith("hotfix/", StringComparison.Ordinal)
                    || branch.StartsWith("bugfix/", StringComparison.Ordinal)
                    || branch.StartsWith("release/", StringComparison.Ordinal)
                )
                {
                    return branch;
                }
            }

            return string.Empty;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to get last branch: {ex.Message}");
            return string.Empty;
        }
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

    private static async Task CreatePullRequestAsync(PullRequestInfo prInfo)
    {
        try
        {
            var existingPrs = await RunProcessAsync("gh", $"pr list --head {prInfo.SourceBranch} --state all");
            if (!string.IsNullOrEmpty(existingPrs))
            {
                Console.WriteLine($"[INFO] PR already exists or existed for branch {prInfo.SourceBranch}");
                return;
            }

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
