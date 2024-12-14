namespace OnForkHub.Scripts.Git;

public class GitEditorService(string projectRoot, IProcessRunner processRunner, ILogger logger) : IGitEditorService
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
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
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Error configuring editor: {ex.Message}");
            await ConfigureNotepadAsGitEditor();
        }
    }

    private static string GetVsCodePath()
    {
        var possiblePaths = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "Code.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Microsoft VS Code", "Code.exe"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Microsoft VS Code", "Code.exe"),
        };

        var path = Array.Find(possiblePaths, path => !string.IsNullOrEmpty(path) && File.Exists(path));

        return string.IsNullOrEmpty(path) ? throw new FileNotFoundException("VS Code executable not found in standard locations.") : path;
    }

    private async Task ConfigureNotepadAsGitEditor()
    {
        await _processRunner.RunAsync("git", "config --local core.editor \"notepad\"", _projectRoot);
        _logger.Log(ELogLevel.Info, "Notepad configured as Git editor (VSCode not found)");
    }

    private async Task ConfigureVsCodeAsGitEditor()
    {
        var vsCodePath = GetVsCodePath();
        var escapedPath = vsCodePath.Replace("\\", "/");

        var editorCommand = $"config --local core.editor \"\\\"${escapedPath}\\\" --wait\"";

        try
        {
            await _processRunner.RunAsync("git", editorCommand, _projectRoot);
            _logger.Log(ELogLevel.Info, "VSCode configured as Git editor");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to configure VSCode as Git editor: {ex.Message}");
            throw;
        }
    }

    private async Task<bool> IsVsCodeAvailableAsync()
    {
        try
        {
            var vsCodePath = GetVsCodePath();
            await _processRunner.RunAsync(vsCodePath, "--version", _projectRoot);
            return true;
        }
        catch
        {
            return false;
        }
    }
}