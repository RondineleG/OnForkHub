// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Scripts.Husky;

public sealed class HuskyConfiguration(
    string projectRoot,
    ILogger logger,
    IProcessRunner processRunner,
    IGitEditorService gitEditorService,
    GitFlowConfiguration gitFlowConfiguration
)
{
    private readonly IGitEditorService _gitEditorService = gitEditorService ?? throw new ArgumentNullException(nameof(gitEditorService));

    private readonly GitFlowConfiguration _gitFlowConfiguration =
        gitFlowConfiguration ?? throw new ArgumentNullException(nameof(gitFlowConfiguration));

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    private readonly string _projectRoot = projectRoot ?? throw new ArgumentNullException(nameof(projectRoot));

    public async Task<bool> ConfigureAsync()
    {
        _logger.Log(ELogLevel.Info, "Starting Husky configuration...");
        var huskyPath = Path.Combine(_projectRoot, ".husky");
        _logger.Log(ELogLevel.Info, $"Husky Path: {huskyPath}");

        try
        {
            if (!await _gitFlowConfiguration.VerifyGitInstallationAsync())
            {
                _logger.Log(ELogLevel.Error, "Git installation verification failed.");
                return false;
            }

            await _gitFlowConfiguration.EnsureCleanWorkingTreeAsync();
            await _gitFlowConfiguration.EnsureGitFlowConfiguredAsync();

            if (!await ConfigureHuskyToolsAsync())
            {
                return false;
            }

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
}
