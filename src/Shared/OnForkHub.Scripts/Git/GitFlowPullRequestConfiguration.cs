namespace OnForkHub.Scripts.Git;

public static class GitFlowPullRequestConfiguration
{
    private static async Task PushBranch(string branch)
    {
        var branchName = branch.StartsWith("feature/", StringComparison.OrdinalIgnoreCase) ? branch[8..] : branch;
        await RunProcessAsync("git", $"flow feature publish {branchName}");
    }

    public static async Task CreatePullRequestForGitFlowFinishAsync()
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

        await PushBranch(branchName);

        if (IsInMergeProcess())
        {
            Console.WriteLine("[DEBUG] Already in merge process, skipping");
            return;
        }

        Console.WriteLine("[DEBUG] Starting CreatePullRequestForGitFlowFinishAsync");

        if (await PullRequestExists(branchName))
        {
            Console.WriteLine("[INFO] Pull request already exists");
            return;
        }

        Console.WriteLine("[DEBUG] Creating pull request");

        await AuthenticateWithGitHubCliAsync();

        await CreatePullRequestWithGitHubCLIAsync(
            new PullRequestInfo(
                $"Merge {branchName} into dev",
                $"Automatically generated PR for merging branch {branchName} into dev .",
                branchName,
                "dev"
            )
        );
    }

    private static bool IsInMergeProcess()
    {
        try
        {
            var result = RunProcessAsync("git", "rev-parse -q --verify MERGE_HEAD").Result;
            return !string.IsNullOrWhiteSpace(result);
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

    private static async Task<bool> PullRequestExists(string branchName)
    {
        try
        {
            var result = await RunProcessAsync("gh", $"pr list --head {branchName} --base dev --state open");
            return !string.IsNullOrWhiteSpace(result);
        }
        catch
        {
            return false;
        }
    }

    private static async Task AuthenticateWithGitHubCliAsync()
    {
        await RunProcessAsync("gh", "auth status");
    }

    private static async Task CreatePullRequestWithGitHubCLIAsync(PullRequestInfo prInfo)
    {
        try
        {
            var command = $"pr create --title \"{prInfo.Title}\" --body \"{prInfo.Body}\" --base {prInfo.BaseBranch} --head {prInfo.SourceBranch}";
            Console.WriteLine($"[DEBUG] Creating PR with command: gh {command}");
            var result = await RunProcessAsync("gh", command);
            Console.WriteLine($"[INFO] Successfully created PR: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Could not create PR: {ex.Message}");
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
