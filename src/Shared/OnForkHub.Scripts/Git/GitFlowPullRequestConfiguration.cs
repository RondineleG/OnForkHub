using System.Globalization;

namespace OnForkHub.Scripts.Git;

public static class GitFlowPullRequestConfiguration
{
    public static async Task CreatePullRequestForGitFlowFinishAsync()
    {
        try
        {
            var branchName = await GetSourceBranch();
            if (string.IsNullOrEmpty(branchName))
            {
                Console.WriteLine("[INFO] No branch name found");
                return;
            }

            if (!branchName.StartsWith("feature/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            try
            {
                Console.WriteLine($"[INFO] Syncing feature branch {branchName}");
                await RunProcessAsync("git", $"fetch origin {branchName}");
                await RunProcessAsync("git", $"push origin {branchName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not sync feature branch: {ex.Message}");
            }

            if (!await HasCommits(branchName))
            {
                Console.WriteLine("[INFO] No commits to create PR");
                return;
            }

            await PushBranch(branchName);

            var prInfo = new PullRequestInfo(
                $"Merge {branchName} into dev",
                $"Automatically generated PR for merging branch {branchName} into dev.",
                "dev",
                branchName
            );

            await CreatePullRequestWithGitHubCLIAsync(prInfo);

            try
            {
                await RunProcessAsync("git", "merge --abort");
            }
            catch { }

            try
            {
                await RunProcessAsync("git", $"checkout {branchName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Could not checkout feature branch: {ex.Message}");
            }

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error in CreatePullRequestForGitFlowFinishAsync: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task PushBranch(string branch)
    {
        try
        {
            await RunProcessAsync("git", $"fetch origin");
            await RunProcessAsync("git", $"push -f origin {branch}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to push branch: {ex.Message}");
            throw;
        }
    }

    private static async Task<bool> HasCommits(string sourceBranch, string targetBranch = "dev")
    {
        try
        {
            var result = await RunProcessAsync("git", $"rev-list --count {targetBranch}..{sourceBranch}");
            return int.Parse(result, CultureInfo.InvariantCulture) > 0;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<string> GetSourceBranch()
    {
        return await RunProcessAsync("git", "rev-parse --abbrev-ref HEAD");
    }

    private static async Task CreatePullRequestWithGitHubCLIAsync(PullRequestInfo prInfo)
    {
        try
        {
            var existingPRs = await RunProcessAsync("gh", $"pr list --head {prInfo.SourceBranch} --base {prInfo.BaseBranch} --state open");

            if (!string.IsNullOrWhiteSpace(existingPRs))
            {
                var prNumber = existingPRs.Split('\t')[0];
                await RunProcessAsync("gh", $"pr edit {prNumber} --title \"{prInfo.Title}\" --body \"{prInfo.Body}\"");
                Console.WriteLine($"[INFO] Updated existing PR #{prNumber}");
                return;
            }

            var command = $"pr create --title \"{prInfo.Title}\" --body \"{prInfo.Body}\" --base {prInfo.BaseBranch} --head {prInfo.SourceBranch}";
            var result = await RunProcessAsync("gh", command);
            Console.WriteLine($"[INFO] Successfully created PR: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Could not create/update PR: {ex.Message}");
            throw;
        }
    }

    private static async Task<string> RunProcessAsync(string command, string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        process.Start();
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        return process.ExitCode != 0
            ? throw new InvalidOperationException($"Command '{command} {arguments}' failed with error: {error}")
            : output.Trim();
    }
}
