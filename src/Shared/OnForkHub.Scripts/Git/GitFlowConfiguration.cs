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

            try
            {
                await _processRunner.RunAsync("git", "flow init -d");
                _logger.Log(ELogLevel.Info, "Git Flow initialized with default settings.");
            }
            catch
            {
                _logger.Log(ELogLevel.Info, "Default initialization failed, trying manual configuration...");
            }

            await ConfigureGitFlowSettingsAsync();

            await EnsureDevBranchExistsAsync();

            _logger.Log(ELogLevel.Info, "Git Flow configuration completed.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "An error occurred while configuring Git Flow:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw new GitOperationException("Failed to configure Git Flow.", ex);
        }
    }

    private async Task ConfigureGitFlowSettingsAsync()
    {
        foreach (var config in _defaultConfig)
        {
            try
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to set {config.Key}: {ex.Message}");
            }
        }

        var additionalConfigs = new Dictionary<string, string>
        {
            { "gitflow.feature.finish", "false" },
            { "gitflow.feature.no-ff", "true" },
            { "gitflow.feature.no-merge", "true" },
            { "gitflow.feature.keepbranch", "true" },
        };

        foreach (var config in additionalConfigs)
        {
            try
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to set {config.Key}: {ex.Message}");
            }
        }
    }

    private async Task EnsureDevBranchExistsAsync()
    {
        try
        {
            var branches = await _processRunner.RunAsync("git", "branch -a");
            if (!branches.Contains("dev") && !branches.Contains("develop"))
            {
                await _processRunner.RunAsync("git", "checkout -b dev");
                _logger.Log(ELogLevel.Info, "Created dev branch");

                try
                {
                    await _processRunner.RunAsync("git", "push -u origin dev");
                    _logger.Log(ELogLevel.Info, "Pushed dev branch to remote");
                }
                catch
                {
                    _logger.Log(ELogLevel.Warning, "Could not push dev branch to remote. This is normal for new repositories.");
                }

                await _processRunner.RunAsync("git", "checkout -");
            }
            else
            {
                _logger.Log(ELogLevel.Info, "Dev branch already exists");
            }
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Error managing dev branch: {ex.Message}");
            throw;
        }
    }
}
