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
                await ConfigureProfileAsync(psProfilePath);
            }

            var reloadScript =
                @"
            $env:Path = [System.Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path', 'User')
            if (Test-Path $PROFILE) { . $PROFILE }
            Get-Alias | Where-Object { $_.Name -like 'g*' } | Format-Table -AutoSize
        ";

            foreach (var shell in new[] { "powershell", "pwsh" })
            {
                try
                {
                    await _processRunner.RunAsync(shell, $"-NoProfile -Command {reloadScript}");
                    _logger.Log(ELogLevel.Info, $"Aliases loaded in {shell}");
                }
                catch { }
            }

            _logger.Log(ELogLevel.Info, "PowerShell Git aliases configured successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Failed to configure PowerShell aliases: {ex.Message}");
        }
    }

    private async Task ConfigureProfileAsync(string profilePath)
    {
        var profileDir = Path.GetDirectoryName(profilePath);
        if (!Directory.Exists(profileDir))
        {
            Directory.CreateDirectory(profileDir!);
        }

        var aliasContent = GeneratePowerShellAliasContent();

        if (File.Exists(profilePath))
        {
            var currentContent = await File.ReadAllTextAsync(profilePath);
            if (!currentContent.Contains("# Git Aliases"))
            {
                await File.AppendAllTextAsync(profilePath, $"\n{aliasContent}");
            }
            else
            {
                var newContent = UpdateExistingAliases(currentContent, aliasContent);
                await File.WriteAllTextAsync(profilePath, newContent);
            }
        }
        else
        {
            await File.WriteAllTextAsync(profilePath, aliasContent);
        }

        _logger.Log(ELogLevel.Info, $"Configured aliases in: {profilePath}");
    }

    private static string UpdateExistingAliases(string currentContent, string newAliases)
    {
        var lines = currentContent.Split('\n');
        var startIndex = Array.FindIndex(lines, l => l.Contains("# Git Aliases"));
        var endIndex = Array.FindIndex(lines, startIndex + 1, l => l.Contains("}}"));

        if (startIndex >= 0 && endIndex >= 0)
        {
            var beforeAliases = string.Join('\n', lines.Take(startIndex));
            var afterAliases = string.Join('\n', lines.Skip(endIndex + 1));
            return $"{beforeAliases}\n{newAliases}{afterAliases}";
        }

        return currentContent + "\n" + newAliases;
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

    function GitLog {{ 
        & git log --graph --pretty=format:'%C(red)%h%C(reset) - %C(yellow)%d%C(reset) %s %C(green)(%cr) %C(bold blue)<%an>%C(reset)' --abbrev-commit $args 
    }}
    Set-Alias -Name gl -Value GitLog -Force -Option AllScope

    # Force reload the alias
    if (Test-Path alias:gl) {{
        Remove-Item alias:gl -Force
        Set-Alias -Name gl -Value GitLog -Force -Option AllScope
    }}

    Write-Host 'Git aliases loaded successfully!'
}}";
    }
}
