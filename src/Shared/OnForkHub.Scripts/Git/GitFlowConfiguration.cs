using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Git;

public sealed class GitFlowConfiguration(ILogger logger, IProcessRunner processRunner)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    public async Task<bool> VerifyGitInstallationAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Checking Git installation...");
            var gitVersion = await _processRunner.RunAsync("git", "--version");
            _logger.Log(ELogLevel.Info, $"Git Version: {gitVersion.Trim()}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "Failed to verify Git installation:");
            _logger.Log(ELogLevel.Error, ex.Message);
            return false;
        }
    }

    public async Task EnsureCleanWorkingTreeAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Checking for unstaged changes...");
            var statusOutput = await _processRunner.RunAsync("git", "status --porcelain");

            if (!string.IsNullOrWhiteSpace(statusOutput))
            {
                const string errorMessage = "Working tree contains unstaged changes. Please commit or stash changes before proceeding.";
                _logger.Log(ELogLevel.Error, errorMessage);
                throw new GitOperationException(errorMessage);
            }

            _logger.Log(ELogLevel.Info, "Working tree is clean.");
        }
        catch (Exception ex) when (ex is not GitOperationException)
        {
            _logger.Log(ELogLevel.Error, "Failed to verify clean working tree:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw new GitOperationException("Failed to verify working tree state.", ex);
        }
    }

    public async Task EnsureGitFlowConfiguredAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Initializing Git Flow...");

            var currentBranch = await _processRunner.RunAsync("git", "rev-parse --abbrev-ref HEAD");
            currentBranch = currentBranch.Trim();

            await CreateBranchIfNotExists("main");

            await CreateBranchIfNotExists("dev");

            await ConfigureGitFlow();

            if (currentBranch is not "main" and not "dev")
            {
                await _processRunner.RunAsync("git", $"checkout {currentBranch}");
            }

            _logger.Log(ELogLevel.Info, "Git Flow configuration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "An error occurred while configuring Git Flow:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw;
        }
    }

    private async Task CreateBranchIfNotExists(string branchName)
    {
        try
        {
            var branches = await _processRunner.RunAsync("git", "branch");
            if (!branches.Contains(branchName))
            {
                _logger.Log(ELogLevel.Info, $"Creating {branchName} branch...");
                await _processRunner.RunAsync("git", $"checkout -b {branchName}");
                try
                {
                    await _processRunner.RunAsync("git", $"push -u origin {branchName}");
                }
                catch
                {
                    _logger.Log(ELogLevel.Warning, $"Could not push {branchName} branch to remote. This is normal for new repositories.");
                }
            }
            else
            {
                await _processRunner.RunAsync("git", $"checkout {branchName}");
            }
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error creating/checking out {branchName} branch: {ex.Message}");
            throw;
        }
    }

    private async Task ConfigureGitFlow()
    {
        var configs = new Dictionary<string, string>
        {
            { "gitflow.branch.master", "main" },
            { "gitflow.branch.develop", "dev" },
            { "gitflow.prefix.feature", "feature/" },
            { "gitflow.prefix.bugfix", "bugfix/" },
            { "gitflow.prefix.release", "release/" },
            { "gitflow.prefix.hotfix", "hotfix/" },
            { "gitflow.prefix.support", "support/" },
            { "gitflow.prefix.versiontag", "v" },
            { "gitflow.feature.start.fetch", "true" },
            { "gitflow.feature.finish.fetch", "true" },
            { "gitflow.feature.finish", "false" },
            { "gitflow.feature.no-ff", "true" },
            { "gitflow.feature.no-merge", "true" },
            { "gitflow.feature.keepbranch", "true" },
            { "gitflow.path.hooks", ".husky" },
        };

        foreach (var config in configs)
        {
            try
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}");
                _logger.Log(ELogLevel.Info, $"Set {config.Key} to {config.Value}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to set {config.Key}: {ex.Message}");
            }
        }

        try
        {
            await _processRunner.RunAsync("git", "flow init -d");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Git flow init warning (this is usually ok): {ex.Message}");
        }
    }
}
