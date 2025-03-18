namespace OnForkHub.Scripts.Git;

public sealed class GitFlowPullRequestConfiguration(ILogger logger, IProcessRunner processRunner, IGitHubClient githubClient)
{
    private const string DevBranch = "dev";

    private const int MaxRetries = 3;

    private readonly IGitHubClient _githubClient = githubClient ?? throw new ArgumentNullException(nameof(githubClient));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    public async Task CreatePullRequestForGitFlowFinishAsync()
    {
        try
        {
            var branchName = await GetCurrentBranchAsync();
            if (!IsFeatureBranch(branchName))
            {
                _logger.Log(ELogLevel.Info, "Current branch is not a feature branch. Skipping PR creation.");
                return;
            }

            _logger.Log(ELogLevel.Info, $"Starting PR creation for {branchName}");

            await SwitchToBranchAsync(branchName);
            await ForcePushFeatureBranchWithRetryAsync(branchName);

            var prInfo = new PullRequestInfo(
                $"feat({GetFeatureName(branchName)}): Merge {branchName} into {DevBranch}",
                GeneratePullRequestBody(branchName),
                DevBranch,
                branchName
            );

            await CreateOrUpdatePullRequestAsync(prInfo);
            await AbortMergeAsync();

            await SwitchToBranchAsync(branchName);
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error in CreatePullRequestForGitFlowFinishAsync: {ex.Message}");
            throw;
        }
    }

    private static string GeneratePullRequestBody(string branchName)
    {
        return $"""
            ## Description
            Automatically generated PR for merging branch `{branchName}` into `{DevBranch}`.

            ## Changes
            - Implementation of {GetFeatureName(branchName)}

            ## Testing
            - [ ] Unit Tests
            - [ ] Integration Tests
            - [ ] Manual Testing

            ## Notes
            Please review and provide feedback.
            """;
    }

    private static string GetFeatureName(string branchName)
    {
        return branchName.Replace("feature/", string.Empty, StringComparison.OrdinalIgnoreCase).Replace("-", " ").ToLowerInvariant();
    }

    private static bool IsFeatureBranch(string branchName)
    {
        return !string.IsNullOrEmpty(branchName) && branchName.StartsWith("feature/", StringComparison.OrdinalIgnoreCase);
    }

    private async Task AbortMergeAsync()
    {
        try
        {
            await _processRunner.RunAsync("git", "merge --abort");
            _logger.Log(ELogLevel.Info, "Merge aborted successfully");
        }
        catch
        {
            _logger.Log(ELogLevel.Info, "No merge to abort");
        }
    }

    private async Task CreateOrUpdatePullRequestAsync(PullRequestInfo prInfo)
    {
        try
        {
            await _githubClient.EnsureLabelsExistAsync();

            var existingPrNumber = await _githubClient.FindExistingPullRequestAsync(prInfo.SourceBranch, prInfo.BaseBranch);

            if (existingPrNumber != null)
            {
                await _githubClient.UpdatePullRequestAsync(existingPrNumber, prInfo);
                _logger.Log(ELogLevel.Info, $"Updated existing PR #{existingPrNumber}");
                return;
            }

            await _githubClient.CreatePullRequestAsync(prInfo);
            _logger.Log(ELogLevel.Info, "Successfully created PR");
        }
        catch (Exception ex)
        {
            throw new GitOperationException("Could not create/update PR", ex);
        }
    }

    private async Task ForcePushFeatureBranchWithRetryAsync(string branchName)
    {
        var attempts = 0;
        while (attempts < MaxRetries)
        {
            try
            {
                attempts++;
                _logger.Log(ELogLevel.Info, $"Force pushing {branchName} (attempt {attempts}/{MaxRetries})...");

                await _processRunner.RunAsync("git", "fetch origin");

                await _processRunner.RunAsync("git", $"push -f origin {branchName}");

                _logger.Log(ELogLevel.Info, $"Successfully pushed {branchName}");
                return;
            }
            catch (Exception ex)
            {
                if (attempts == MaxRetries)
                {
                    throw new GitOperationException($"Failed to push branch {branchName} after {MaxRetries} attempts", ex);
                }

                _logger.Log(ELogLevel.Warning, $"Push attempt {attempts} failed, retrying...");
                await Task.Delay(1000 * attempts);
            }
        }
    }

    private async Task<string> GetCurrentBranchAsync()
    {
        var branch = await _processRunner.RunAsync("git", "rev-parse --abbrev-ref HEAD");
        return branch.Trim();
    }

    private async Task SwitchToBranchAsync(string branchName)
    {
        try
        {
            await _processRunner.RunAsync("git", $"checkout {branchName}");
            _logger.Log(ELogLevel.Info, $"Switched to branch {branchName}");
        }
        catch (Exception ex)
        {
            throw new GitOperationException($"Failed to switch to branch {branchName}", ex);
        }
    }
}
