namespace OnForkHub.Scripts.Git;

public partial class PullRequestConfiguration
{
    [GeneratedRegex(@"from (feature/[^:\s]+|hotfix/[^:\s]+|bugfix/[^:\s]+|release/[^:\s]+)")]
    public static partial Regex BranchNameRegex();

    public static async Task CreatePullRequestForGitFlowFinishAsync()
    {
        try
        {
            var gitLog = await RunProcessAsync("git", "reflog -1");
            Console.WriteLine($"[DEBUG] Git reflog: {gitLog}");

            var match = BranchNameRegex().Match(gitLog);
            if (!match.Success)
            {
                Console.WriteLine("[INFO] No matching branch found in git reflog");
                return;
            }

            var sourceBranch = match.Groups[1].Value;
            Console.WriteLine($"[INFO] Source branch: {sourceBranch}");

            var prInfo = await GetPullRequestInfoAsync(sourceBranch);
            if (prInfo is null)
            {
                Console.WriteLine("[INFO] Branch type not recognized");
                return;
            }

            await CreatePullRequestWithGitHubCLIAsync(prInfo);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Failed to create pull request: {ex.Message}");
        }
    }

    private static async Task<PullRequestInfo?> GetPullRequestInfoAsync(string branchName)
    {
        try
        {
            if (string.IsNullOrEmpty(branchName))
            {
                return null;
            }

            var info = new PullRequestInfo(branchName);

            if (branchName.StartsWith("feature/", StringComparison.Ordinal))
            {
                info.Title = $"Feature completed: {branchName.Replace("feature/", "", StringComparison.Ordinal)}";
                info.Body = "This feature is ready for review.";
                info.BaseBranch = "dev";

                await RunProcessAsync("git", $"checkout {branchName}");
                await RunProcessAsync("git", "pull origin dev --rebase");
            }
            else if (branchName.StartsWith("hotfix/", StringComparison.Ordinal))
            {
                info.Title = $"Hotfix: {branchName.Replace("hotfix/", "", StringComparison.Ordinal)}";
                info.Body = "Urgent hotfix for production.";
                info.BaseBranch = "main";
                await RunProcessAsync("git", $"checkout {branchName}");
                await RunProcessAsync("git", "pull origin main --rebase");
            }
            else if (branchName.StartsWith("bugfix/", StringComparison.Ordinal))
            {
                info.Title = $"Bugfix: {branchName.Replace("bugfix/", "", StringComparison.Ordinal)}";
                info.Body = "Bug fixed and ready for review.";
                info.BaseBranch = "dev";
                await RunProcessAsync("git", $"checkout {branchName}");
                await RunProcessAsync("git", "pull origin dev --rebase");
            }
            else if (branchName.StartsWith("release/", StringComparison.Ordinal))
            {
                info.Title = $"Release: {branchName.Replace("release/", "", StringComparison.Ordinal)}";
                info.Body = "Release is ready for production.";
                info.BaseBranch = "main";
                await RunProcessAsync("git", $"checkout {branchName}");
                await RunProcessAsync("git", "pull origin main --rebase");
            }
            else
            {
                return null;
            }

            return info;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Error preparing PR info: {ex.Message}");
            return null;
        }
    }

    private static async Task CreatePullRequestWithGitHubCLIAsync(PullRequestInfo prInfo)
    {
        try
        {
            var ghVersion = await RunProcessAsync("gh", "--version");
            Console.WriteLine($"[INFO] GitHub CLI version: {ghVersion}");

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
            Console.WriteLine($"[ERROR] Failed to create PR with GitHub CLI: {ex.Message}");
            throw;
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
