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

        var complexAliases = new Dictionary<string, string>
        {
            { "gc", "!sh -c 'git commit -m \"$*\"' -" },
            { "gt", "!sh -c 'git log --max-count=${1:-10} --graph --oneline --decorate' -" },
            {
                "gl",
                "!sh -c 'git log --max-count=${1:-10} --graph --pretty=format:\"%C(red)%h%C(reset) - %C(yellow)%d%C(reset) %s %C(green)(%cr) %C(bold blue)<%an>%C(reset)\" --abbrev-commit' -"
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
Remove-Item Alias:gc -Force -ErrorAction SilentlyContinue
Remove-Item Alias:gps -Force -ErrorAction SilentlyContinue
Remove-Item Alias:gl -Force -ErrorAction SilentlyContinue
Remove-Item Alias:gt -Force -ErrorAction SilentlyContinue

function GitStatus { git status -sb $args }
Set-Alias -Name gs -Value GitStatus -Force -Option AllScope

function GitCommit {
    if ($args.Count -eq 0) {
        Write-Host ""Usage: gc 'commit message'"" -ForegroundColor Yellow
        return
    }
    
    # Handle both with and without -m flag
    if ($args[0] -eq ""-m"") {
        if ($args.Count -lt 2) {
            Write-Host ""Error: No commit message provided"" -ForegroundColor Red
            return
        }
        $commitMessage = $args[1]
    } else {
        $commitMessage = $args -join ' '
    }
    
    # Ensure message is properly quoted
    git commit -m ""$commitMessage""
}
Set-Alias -Name gc -Value GitCommit -Force -Option AllScope

function GitAdd { git add --all $args }
Set-Alias -Name ga -Value GitAdd -Force -Option AllScope

function GitTree {
    $count = 10
    if ($args.Count -gt 0 -and [int]::TryParse($args[0], [ref]$count)) {
        git log --max-count=$count --graph --oneline --decorate
    } else {
        git log --max-count=$count --graph --oneline --decorate
    }
}
Set-Alias -Name gt -Value GitTree -Force -Option AllScope

function GitLog {
    $count = 10
    if ($args.Count -gt 0 -and [int]::TryParse($args[0], [ref]$count)) {
        git log --max-count=$count --graph --pretty=format:'%C(red)%h%C(reset) - %C(yellow)%d%C(reset) %s %C(green)(%cr) %C(bold blue)<%an>%C(reset)' --abbrev-commit
    } else {
        git log --max-count=$count --graph --pretty=format:'%C(red)%h%C(reset) - %C(yellow)%d%C(reset) %s %C(green)(%cr) %C(bold blue)<%an>%C(reset)' --abbrev-commit
    }
}
Set-Alias -Name gl -Value GitLog -Force -Option AllScope

function GitPush { git push $args }
Set-Alias -Name gps -Value GitPush -Force -Option AllScope

function GitPull { git pull $args }
Set-Alias -Name gpl -Value GitPull -Force -Option AllScope

function GitFetch { git fetch $args }
Set-Alias -Name gf -Value GitFetch -Force -Option AllScope

function GitCheckout { git checkout $args }
Set-Alias -Name gco -Value GitCheckout -Force -Option AllScope

function GitBranch { git branch $args }
Set-Alias -Name gb -Value GitBranch -Force -Option AllScope

function GitRemote { git remote -v $args }
Set-Alias -Name gr -Value GitRemote -Force -Option AllScope

function GitDiff { git diff $args }
Set-Alias -Name gd -Value GitDiff -Force -Option AllScope";

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
