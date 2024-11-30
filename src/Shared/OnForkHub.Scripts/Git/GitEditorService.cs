using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Git;

public class GitEditorService(string projectRoot, IProcessRunner processRunner, ILogger logger) : IGitEditorService
{
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _projectRoot = projectRoot ?? throw new ArgumentNullException(nameof(projectRoot));

    public async Task ConfigureEditorAsync()
    {
        try
        {
            if (await IsVsCodeAvailableAsync())
            {
                await ConfigureVsCodeAsGitEditor();
            }
            else
            {
                await ConfigureNotepadAsGitEditor();
            }
        }
        catch (Exception)
        {
            await ConfigureNotepadAsGitEditor();
        }
    }

    private async Task<bool> IsVsCodeAvailableAsync()
    {
        try
        {
            await _processRunner.RunAsync("code", "--version", _projectRoot);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task ConfigureVsCodeAsGitEditor()
    {
        await _processRunner.RunAsync("git", "config --local core.editor \"code --wait\"", _projectRoot);
        _logger.Log(ELogLevel.Info, "VSCode configured as Git editor");
    }

    private async Task ConfigureNotepadAsGitEditor()
    {
        await _processRunner.RunAsync("git", "config --local core.editor \"notepad\"", _projectRoot);
        _logger.Log(ELogLevel.Info, "Notepad configured as Git editor (VSCode not found)");
    }
}
