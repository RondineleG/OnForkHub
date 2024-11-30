using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Husky;

public sealed class HuskyConfiguration
{
    private readonly ILogger _logger;
    private readonly IProcessRunner _processRunner;
    private readonly IGitEditorService _gitEditorService;
    private readonly GitFlowConfiguration _gitFlowConfiguration;
    private readonly string _projectRoot;

    public HuskyConfiguration(
        string projectRoot,
        ILogger logger,
        IProcessRunner processRunner,
        IGitEditorService gitEditorService,
        GitFlowConfiguration gitFlowConfiguration
    )
    {
        _projectRoot = projectRoot ?? throw new ArgumentNullException(nameof(projectRoot));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
        _gitEditorService = gitEditorService ?? throw new ArgumentNullException(nameof(gitEditorService));
        _gitFlowConfiguration = gitFlowConfiguration ?? throw new ArgumentNullException(nameof(gitFlowConfiguration));
    }

    public async Task<bool> ConfigureAsync()
    {
        _logger.Log(ELogLevel.Info, "Starting Husky configuration...");
        var huskyPath = Path.Combine(_projectRoot, ".husky");
        _logger.Log(ELogLevel.Info, $"Husky Path: {huskyPath}");

        try
        {
            // Use existing GitFlowConfiguration
            if (!await _gitFlowConfiguration.VerifyGitInstallationAsync())
            {
                _logger.Log(ELogLevel.Error, "Git installation verification failed.");
                return false;
            }

            await _gitFlowConfiguration.EnsureCleanWorkingTreeAsync();
            await _gitFlowConfiguration.EnsureGitFlowConfiguredAsync();

            // Configure Husky specific tools
            if (!await ConfigureHuskyToolsAsync())
            {
                return false;
            }

            // Configure Git Editor
            await _gitEditorService.ConfigureEditorAsync();

            _logger.Log(ELogLevel.Info, "Husky configured successfully.");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Husky configuration failed: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> ConfigureHuskyToolsAsync()
    {
        if (!await RestoreDotnetToolsAsync())
        {
            _logger.Log(ELogLevel.Error, "Failed to restore dotnet tools.");
            return false;
        }

        if (!await InstallHuskyAsync())
        {
            _logger.Log(ELogLevel.Error, "Failed to install Husky.");
            return false;
        }

        return true;
    }

    private async Task<bool> RestoreDotnetToolsAsync()
    {
        try
        {
            await _processRunner.RunAsync("dotnet", "tool restore", _projectRoot);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to restore dotnet tools: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> InstallHuskyAsync()
    {
        try
        {
            await _processRunner.RunAsync("dotnet", "husky install", _projectRoot);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to install Husky: {ex.Message}");
            return false;
        }
    }
}
