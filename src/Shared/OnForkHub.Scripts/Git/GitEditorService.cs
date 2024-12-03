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
            var vsCodePath = GetVsCodePath();
            if (string.IsNullOrEmpty(vsCodePath))
            {
                return false;
            }

            await _processRunner.RunAsync(vsCodePath, "--version", _projectRoot);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task ConfigureVsCodeAsGitEditor()
    {
        var vsCodePath = GetVsCodePath();
        if (string.IsNullOrEmpty(vsCodePath))
        {
            throw new InvalidOperationException("VSCode path not found");
        }

        var escapedPath = vsCodePath.Replace("\\", "/");
        await _processRunner.RunAsync("git", $"config --local core.editor \"\"{escapedPath}\" --wait\"", _projectRoot);
        _logger.Log(ELogLevel.Info, "VSCode configured as Git editor");
    }

    private async Task ConfigureNotepadAsGitEditor()
    {
        await _processRunner.RunAsync("git", "config --local core.editor \"notepad\"", _projectRoot);
        _logger.Log(ELogLevel.Info, "Notepad configured as Git editor (VSCode not found)");
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
}
