namespace OnForkHub.Scripts.Git;

public static class GitFlowPullRequestConfiguration
{
    private static async Task<string> GetSourceBranch()
    {
        return await RunProcessAsync("git", "rev-parse --abbrev-ref HEAD");
    }

    private static async Task EnsureLabelsExistAsync()
    {
        var requiredLabels = new Dictionary<string, string>
        {
            { "status:in-review", "6E49CB" },
            { "priority:high", "D93F0B" },
            { "size:large", "2B52D4" },
        };

        foreach (var label in requiredLabels)
        {
            try
            {
                await RunProcessAsync("gh", $"label list --search \"{label.Key}\"");
                var result = await RunProcessAsync("gh", $"label create \"{label.Key}\" --color \"{label.Value}\" --force");
                Console.WriteLine($"[INFO] Label check/creation: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Label creation warning: {ex.Message}");
            }
        }
    }

    private static async Task CreatePullRequestWithGitHubCLIAsync(PullRequestInfo prInfo)
    {
        try
        {
            await EnsureLabelsExistAsync();

            var existingPRs = await RunProcessAsync("gh", $"pr list --head {prInfo.SourceBranch} --base {prInfo.BaseBranch} --state open");
            if (!string.IsNullOrWhiteSpace(existingPRs))
            {
                await RunProcessAsync(
                    "gh",
                    $"pr edit {existingPRs.Split('\t')[0]}"
                        + $" --title \"{prInfo.Title}\""
                        + $" --body \"{prInfo.Body}\""
                        + " --add-label \"status:in-review,priority:high,size:large\""
                        + " --add-assignee @me"
                        + " --milestone onforkhub-core-foundation"
                );

                await RunProcessAsync("gh", $"project-v2 item-add --project OnForkHub --id {existingPRs.Split('\t')[0]} --status \"In Review\"");

                Console.WriteLine($"[INFO] Updated existing PR #{existingPRs.Split('\t')[0]}");
                return;
            }

            var createCommand =
                $"pr create"
                + $" --title \"{prInfo.Title}\""
                + $" --body \"{prInfo.Body}\""
                + $" --base {prInfo.BaseBranch}"
                + $" --head {prInfo.SourceBranch}"
                + " --label \"status:in-review,priority:high,size:large\""
                + " --assignee @me"
                + " --milestone onforkhub-core-foundation";

            var result = await RunProcessAsync("gh", createCommand);

            var prNumber = ExtractPRNumber(result);

            if (!string.IsNullOrEmpty(prNumber))
            {
                await RunProcessAsync("gh", $"project-v2 item-add --project OnForkHub --id {prNumber} --status \"In Review\"");
            }

            Console.WriteLine($"[INFO] Successfully created PR: {result}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Could not create/update PR: {ex.Message}");
            throw;
        }
    }

    private static string ExtractPRNumber(string prUrl)
    {
        var parts = prUrl.TrimEnd().Split('/');
        return parts[^1];
    }

    public static async Task CreatePullRequestForGitFlowFinishAsync()
    {
        try
        {
            var branchName = await GetSourceBranch();
            if (string.IsNullOrEmpty(branchName) || !branchName.StartsWith("feature/", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Console.WriteLine($"[INFO] Starting PR creation for {branchName}");

            await ForcePushFeatureBranch(branchName);

            var prInfo = new PullRequestInfo(
                $"Merge {branchName} into dev",
                $"Automatically generated PR for merging branch {branchName} into dev.",
                "dev",
                branchName
            );

            await CreatePullRequestWithGitHubCLIAsync(prInfo);

            await AbortMerge();

            await RunProcessAsync("git", $"checkout {branchName}");

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error in CreatePullRequestForGitFlowFinishAsync: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static async Task ForcePushFeatureBranch(string branchName)
    {
        try
        {
            Console.WriteLine($"[INFO] Force pushing {branchName}...");

            await RunProcessAsync("git", $"checkout {branchName}");

            await RunProcessAsync("git", $"push -f origin {branchName}");

            Console.WriteLine($"[INFO] Successfully pushed {branchName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to push branch {branchName}: {ex.Message}");
            throw;
        }
    }

    private static async Task AbortMerge()
    {
        try
        {
            await RunProcessAsync("git", "merge --abort");
            Console.WriteLine("[INFO] Merge aborted successfully");
        }
        catch
        {
            Console.WriteLine("[INFO] No merge to abort");
        }
    }

    private static async Task<string> RunProcessAsync(string command, string arguments)
    {
        Console.WriteLine($"[DEBUG] Executing: {command} {arguments}");

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

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"[DEBUG] Process error output: {error}");
        }

        return process.ExitCode != 0
            ? throw new InvalidOperationException($"Command '{command} {arguments}' failed with error: {error}")
            : output.Trim();
    }
}
