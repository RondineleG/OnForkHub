namespace OnForkHub.Scripts.Git;

public sealed class GitAliasConfiguration(ILogger logger, IProcessRunner processRunner) : IGitAliasConfiguration
{
    private readonly Dictionary<string, string> _aliasCommands = new()
    {
        { "gs", "status -sb" },
        { "ga", "add --all" },
        { "gps", "push" },
        { "gpl", "pull" },
        { "gf", "fetch" },
        { "gco", "checkout" },
        { "gb", "branch" },
        { "gr", "remote -v" },
        { "gd", "diff" },
    };

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    public async Task ConfigureAliasesAsync()
    {
        try
        {
            await ConfigureGitAliasesAsync();
            await ConfigurePowerShellAliasesAsync();
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error configuring aliases: {ex.Message}");
            throw;
        }
    }

    private async Task ConfigureGitAliasesAsync()
    {
        foreach (var (alias, command) in _aliasCommands)
        {
            try
            {
                await _processRunner.RunAsync("git", $"config --global alias.{alias} \"{command}\"");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to configure alias {alias}: {ex.Message}");
            }
        }

        // Configure complex aliases separately
        var complexAliases = new Dictionary<string, string>
        {
            { "gc", "!f() { git commit -m \"$*\"; }; f" },
            { "gt", "!f() { n=${1:-10}; git log --graph --oneline --decorate -n \"$n\"; }; f" },
            {
                "gl",
                "!f() { n=${1:-10}; git log --graph --pretty=format:\"%C(red)%h%C(reset) - %C(yellow)%d%C(reset) %s %C(green)(%cr) %C(bold blue)<%an>%C(reset)\" --abbrev-commit -n \"$n\"; }; f"
            },
        };

        foreach (var (alias, command) in complexAliases)
        {
            try
            {
                await _processRunner.RunAsync("git", $"config --global alias.{alias} \"{command}\"");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to configure complex alias {alias}: {ex.Message}");
            }
        }
    }

    private async Task ConfigurePowerShellAliasesAsync()
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

        foreach (var path in profilePaths)
        {
            await ConfigureProfileAsync(path);
        }

        await ReloadProfilesAsync();
    }

    private async Task ReloadProfilesAsync()
    {
        var reloadScript =
            "$env:Path = [System.Environment]::GetEnvironmentVariable('Path', 'Machine') + ';' + [System.Environment]::GetEnvironmentVariable('Path', 'User'); if (Test-Path $PROFILE) { . $PROFILE }";

        foreach (var shell in new[] { "powershell", "pwsh" })
        {
            try
            {
                await _processRunner.RunAsync(shell, $"-NoProfile -Command {reloadScript}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to reload {shell} profile: {ex.Message}");
            }
        }
    }

    private async Task ConfigureProfileAsync(string profilePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(profilePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir!);
            }

            var content =
                @"
function GitCommit { git commit -m ""$($args -join ' ')"" }
function GitTree { $n = if ($args[0]) { $args[0] } else { 10 }; git log --graph --oneline --decorate -n $n }
function GitLog { $n = if ($args[0]) { $args[0] } else { 10 }; git log --graph --pretty=format:'%C(red)%h%C(reset) - %C(yellow)%d%C(reset) %s %C(green)(%cr) %C(bold blue)<%an>%C(reset)' --abbrev-commit -n $n }

Set-Alias -Name gc -Value GitCommit -Force
Set-Alias -Name gt -Value GitTree -Force
Set-Alias -Name gl -Value GitLog -Force
Set-Alias -Name gs -Value 'git status -sb' -Force
Set-Alias -Name ga -Value 'git add --all' -Force
Set-Alias -Name gps -Value 'git push' -Force
Set-Alias -Name gpl -Value 'git pull' -Force
Set-Alias -Name gf -Value 'git fetch' -Force
Set-Alias -Name gco -Value 'git checkout' -Force
Set-Alias -Name gb -Value 'git branch' -Force
Set-Alias -Name gr -Value 'git remote -v' -Force
Set-Alias -Name gd -Value 'git diff' -Force";

            if (File.Exists(profilePath))
            {
                var currentContent = await File.ReadAllTextAsync(profilePath);
                if (!currentContent.Contains("GitCommit"))
                {
                    await File.AppendAllTextAsync(profilePath, content);
                }
            }
            else
            {
                await File.WriteAllTextAsync(profilePath, content);
            }
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Failed to configure profile {profilePath}: {ex.Message}");
        }
    }
}
