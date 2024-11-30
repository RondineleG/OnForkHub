using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;
using OnForkHub.Scripts.Models;

namespace OnForkHub.Scripts.Git;

public sealed class GitFlowPullRequestConfiguration(ILogger logger, IProcessRunner processRunner, IGitHubClient githubClient)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    private readonly IGitHubClient _githubClient = githubClient ?? throw new ArgumentNullException(nameof(githubClient));
    private const string DevBranch = "dev";

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

            await ForcePushFeatureBranchAsync(branchName);

            var prInfo = new PullRequestInfo(
                Title: $"Merge {branchName} into {DevBranch}",
                Body: $"Automatically generated PR for merging branch {branchName} into {DevBranch}.",
                BaseBranch: DevBranch,
                SourceBranch: branchName
            );

            await CreateOrUpdatePullRequestAsync(prInfo);
            await AbortMergeAsync();
            await SwitchToBranchAsync(branchName);

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error in CreatePullRequestForGitFlowFinishAsync: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private async Task<string> GetCurrentBranchAsync()
    {
        return await _processRunner.RunAsync("git", "rev-parse --abbrev-ref HEAD");
    }

    private static bool IsFeatureBranch(string branchName)
    {
        return !string.IsNullOrEmpty(branchName) && branchName.StartsWith("feature/", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ForcePushFeatureBranchAsync(string branchName)
    {
        try
        {
            _logger.Log(ELogLevel.Info, $"Force pushing {branchName}...");
            await SwitchToBranchAsync(branchName);
            await _processRunner.RunAsync("git", $"push -f origin {branchName}");
            _logger.Log(ELogLevel.Info, $"Successfully pushed {branchName}");
        }
        catch (Exception ex)
        {
            throw new GitOperationException($"Failed to push branch {branchName}", ex);
        }
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

    private async Task SwitchToBranchAsync(string branchName)
    {
        await _processRunner.RunAsync("git", $"checkout {branchName}");
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
}
