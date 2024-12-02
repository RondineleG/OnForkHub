using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Git;

public sealed class GitFlowConfiguration(ILogger logger, IProcessRunner processRunner)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    private readonly Dictionary<string, string> _defaultConfig =
        new()
        {
            { "gitflow.branch.master", "main" },
            { "gitflow.branch.develop", "dev" },
            { "gitflow.prefix.feature", "feature/" },
            { "gitflow.prefix.bugfix", "bugfix/" },
            { "gitflow.prefix.release", "release/" },
            { "gitflow.prefix.hotfix", "hotfix/" },
            { "gitflow.prefix.support", "support/" },
            { "gitflow.prefix.versiontag", "v" },
        };

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
            _logger.Log(ELogLevel.Info, "Checking if Git Flow is already configured...");

            // Check if git flow is initialized by checking for the develop branch
            var branches = await _processRunner.RunAsync("git", "branch -a");
            if (!branches.Contains("dev") && !branches.Contains("develop"))
            {
                _logger.Log(ELogLevel.Info, "Git Flow is not configured. Initializing...");
                await InitializeGitFlowAsync();
                _logger.Log(ELogLevel.Info, "Git Flow configured successfully.");
                return;
            }

            _logger.Log(ELogLevel.Info, "Git Flow is already configured.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "An error occurred while checking Git Flow configuration:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw new GitOperationException("Failed to configure Git Flow.", ex);
        }
    }

    private async Task InitializeGitFlowAsync()
    {
        try
        {
            // Create dev branch if it doesn't exist
            await _processRunner.RunAsync("git", "checkout -b dev");

            // Configure git flow settings
            foreach (var config in _defaultConfig)
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}");
            }

            // Additional git flow specific configurations
            await _processRunner.RunAsync("git", "config --local gitflow.feature.finish false");
            await _processRunner.RunAsync("git", "config --local gitflow.feature.no-ff true");
            await _processRunner.RunAsync("git", "config --local gitflow.feature.no-merge true");
            await _processRunner.RunAsync("git", "config --local gitflow.feature.keepbranch true");

            // Push dev branch to remote if it exists
            try
            {
                await _processRunner.RunAsync("git", "push -u origin dev");
            }
            catch
            {
                _logger.Log(ELogLevel.Warning, "Could not push dev branch to remote. This is normal for new repositories.");
            }

            await _processRunner.RunAsync("git", "checkout main");
        }
        catch (Exception ex)
        {
            throw new GitOperationException("Failed to initialize Git Flow.", ex);
        }
    }
}
