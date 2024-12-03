using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Git;

public sealed class GitAliasConfiguration(ILogger logger, IProcessRunner processRunner) : IGitAliasConfiguration
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    private readonly Dictionary<string, string> _aliasCommands =
        new()
        {
            { "gs", "status -sb" },
            { "gc", "commit -ev" },
            { "ga", "add --all" },
            { "gt", "log --graph --oneline --decorate" },
            { "gps", "push" },
            { "gpl", "pull" },
            { "gf", "fetch" },
            { "gco", "checkout" },
            { "gb", "branch" },
            { "gr", "remote -v" },
            { "gd", "diff" },
            { "gl", "log --pretty=format:'%h %ad | %s%d [%an]' --graph --date=short" },
        };

    public async Task ConfigureAliasesAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Configuring Git aliases...");

            foreach (var (alias, command) in _aliasCommands)
            {
                try
                {
                    await _processRunner.RunAsync("git", $"config --global alias.{alias} \"{command}\"");
                    _logger.Log(ELogLevel.Info, $"Configured alias: {alias} => git {command}");
                }
                catch (Exception ex)
                {
                    _logger.Log(ELogLevel.Warning, $"Failed to configure alias {alias}: {ex.Message}");
                }
            }

            await ConfigurePowerShellAliasesAsync();

            _logger.Log(ELogLevel.Info, "Git aliases configuration completed.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error configuring Git aliases: {ex.Message}");
            throw;
        }
    }

    private async Task ConfigurePowerShellAliasesAsync()
    {
        try
        {
            var profilePaths = new[]
            {
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Documents",
                    "WindowsPowerShell",
                    "Microsoft.PowerShell_profile.ps1"
                ),
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Documents",
                    "PowerShell",
                    "Microsoft.PowerShell_profile.ps1"
                ),
            };

            foreach (var psProfilePath in profilePaths)
            {
                var profileDir = Path.GetDirectoryName(psProfilePath);
                if (!Directory.Exists(profileDir))
                {
                    Directory.CreateDirectory(profileDir!);
                }

                var aliasContent = GeneratePowerShellAliasContent();

                if (File.Exists(psProfilePath))
                {
                    var currentContent = await File.ReadAllTextAsync(psProfilePath);
                    if (!currentContent.Contains("# Git Aliases"))
                    {
                        await File.AppendAllTextAsync(psProfilePath, $"\n{aliasContent}");
                        _logger.Log(ELogLevel.Info, $"Added aliases to existing profile: {psProfilePath}");
                    }
                }
                else
                {
                    await File.WriteAllTextAsync(psProfilePath, aliasContent);
                    _logger.Log(ELogLevel.Info, $"Created new profile with aliases: {psProfilePath}");
                }
            }

            foreach (var shell in new[] { "powershell", "pwsh" })
            {
                try
                {
                    await _processRunner.RunAsync(shell, "-NoProfile -Command . $PROFILE -Force");
                }
                catch { }
            }

            _logger.Log(
                ELogLevel.Info,
                @"PowerShell Git aliases configured successfully.
To use the aliases immediately, please:
1. Close and reopen your PowerShell window, or
2. Run this command: . $PROFILE"
            );
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Failed to configure PowerShell aliases: {ex.Message}");
        }
    }

    private static string GeneratePowerShellAliasContent()
    {
        return $@"
# Git Aliases
# Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}
if (Get-Command git -ErrorAction SilentlyContinue) {{
    function GitStatus {{ & git status -sb $args }}
    Set-Alias -Name gs -Value GitStatus -Force -Option AllScope
    
    function GitCommit {{ & git commit -ev $args }}
    Set-Alias -Name gc -Value GitCommit -Force -Option AllScope
    
    function GitAdd {{ & git add --all $args }}
    Set-Alias -Name ga -Value GitAdd -Force -Option AllScope
    
    function GitTree {{ & git log --graph --oneline --decorate $args }}
    Set-Alias -Name gt -Value GitTree -Force -Option AllScope
    
    function GitPush {{ & git push $args }}
    Set-Alias -Name gps -Value GitPush -Force -Option AllScope
    
    function GitPull {{ & git pull $args }}
    Set-Alias -Name gpl -Value GitPull -Force -Option AllScope
    
    function GitFetch {{ & git fetch $args }}
    Set-Alias -Name gf -Value GitFetch -Force -Option AllScope
    
    function GitCheckout {{ & git checkout $args }}
    Set-Alias -Name gco -Value GitCheckout -Force -Option AllScope
    
    function GitBranch {{ & git branch $args }}
    Set-Alias -Name gb -Value GitBranch -Force -Option AllScope
    
    function GitRemote {{ & git remote -v $args }}
    Set-Alias -Name gr -Value GitRemote -Force -Option AllScope
    
    function GitDiff {{ & git diff $args }}
    Set-Alias -Name gd -Value GitDiff -Force -Option AllScope

    Write-Host 'Git aliases loaded successfully!'
}}";
    }
}
