namespace OnForkHub.Scripts;

public class Startup(
    ILogger logger,
    GitFlowConfiguration gitFlowConfig,
    HuskyConfiguration huskyConfig,
    GitFlowPullRequestConfiguration prConfig,
    IGitAliasConfiguration aliasConfig
)
{
    private readonly ILogger _logger = logger;
    private readonly GitFlowConfiguration _gitFlowConfig = gitFlowConfig;
    private readonly HuskyConfiguration _huskyConfig = huskyConfig;
    private readonly GitFlowPullRequestConfiguration _prConfig = prConfig;
    private readonly IGitAliasConfiguration _aliasConfig = aliasConfig;

    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            _logger.Log(ELogLevel.Info, $"Starting application with args: {string.Join(" ", args)}");

            if (!await _gitFlowConfig.VerifyGitInstallationAsync())
            {
                _logger.Log(ELogLevel.Error, "Git not installed.");
                return 1;
            }

            await ConfigureGitFlowAsync();
            await _aliasConfig.ConfigureAliasesAsync();

            if (!await ConfigureHuskyAsync())
            {
                return 1;
            }

            if (args.Contains("pr-create"))
            {
                await HandlePullRequestCreationAsync();
            }
            else
            {
                _logger.Log(ELogLevel.Info, "Skipping PR creation (pr-create flag not present)");
            }

            _logger.Log(ELogLevel.Info, "Configuration completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"An unexpected error occurred: {ex.Message}");
            _logger.Log(ELogLevel.Debug, $"Stack Trace: {ex.StackTrace}");
            return 1;
        }
    }

    private async Task ConfigureGitFlowAsync()
    {
        await _gitFlowConfig.EnsureCleanWorkingTreeAsync();
        await _gitFlowConfig.EnsureGitFlowConfiguredAsync();
        _logger.Log(ELogLevel.Info, "Git Flow configuration completed successfully.");
    }

    private async Task<bool> ConfigureHuskyAsync()
    {
        if (!await _huskyConfig.ConfigureAsync())
        {
            _logger.Log(ELogLevel.Error, "Husky configuration failed.");
            return false;
        }
        return true;
    }

    private async Task HandlePullRequestCreationAsync()
    {
        try
        {
            await _prConfig.CreatePullRequestForGitFlowFinishAsync();
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to create pull request: {ex.Message}");
            throw;
        }
    }
}
