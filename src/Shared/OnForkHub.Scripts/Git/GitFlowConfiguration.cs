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
            _logger.Log(ELogLevel.Info, "Checking if Git Flow is already configured...");
            await _processRunner.RunAsync("git", "flow config");
            _logger.Log(ELogLevel.Info, "Git Flow is already configured.");
        }
        catch (GitOperationException ex) when (ex.Message.Contains("Not a gitflow-enabled repo", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Log(ELogLevel.Info, "Git Flow is not configured. Initializing...");
            await _processRunner.RunAsync("git", "flow init -d");
            _logger.Log(ELogLevel.Info, "Git Flow configured successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "An error occurred while checking Git Flow configuration:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw new GitOperationException("Failed to configure Git Flow.", ex);
        }
    }
}
