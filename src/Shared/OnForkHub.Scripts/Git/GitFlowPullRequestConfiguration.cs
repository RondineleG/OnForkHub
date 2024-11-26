namespace OnForkHub.Scripts.Git;

public static class GitFlowPullRequestConfiguration
{
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

            // Força push da branch feature antes de qualquer operação
            await ForcePushFeatureBranch(branchName);

            var prInfo = new PullRequestInfo(
                $"Merge {branchName} into dev",
                $"Automatically generated PR for merging branch {branchName} into dev.",
                "dev",
                branchName
            );

            await CreatePullRequestWithGitHubCLIAsync(prInfo);

            // Aborta qualquer merge em andamento
            await AbortMerge();

            // Mantém na branch feature
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

            // Garante que estamos na branch correta
            await RunProcessAsync("git", $"checkout {branchName}");

            // Força o push da feature branch
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
            // Ignora erro se não houver merge em andamento
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
