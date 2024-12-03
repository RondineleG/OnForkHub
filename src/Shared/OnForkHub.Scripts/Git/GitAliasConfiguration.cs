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
            var psProfilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Documents",
                "WindowsPowerShell",
                "Microsoft.PowerShell_profile.ps1"
            );

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
                }
            }
            else
            {
                await File.WriteAllTextAsync(psProfilePath, aliasContent);
            }

            try
            {
                await _processRunner.RunAsync("powershell", "-NoProfile -Command & {. $PROFILE}");
                _logger.Log(ELogLevel.Info, "PowerShell profile reloaded successfully.");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to reload PowerShell profile: {ex.Message}");
                _logger.Log(ELogLevel.Info, "Please restart your PowerShell session or run '. $PROFILE' to load the new aliases.");
            }

            _logger.Log(ELogLevel.Info, "PowerShell Git aliases configured successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Failed to configure PowerShell aliases: {ex.Message}");
        }
    }

    private static string GeneratePowerShellAliasContent()
    {
        return """
            # Git Aliases
            function GitStatus { & git status -sb $args }
            New-Alias -Name gs -Value GitStatus -Force -Option AllScope

            function GitCommit { & git commit -ev $args }
            New-Alias -Name gc -Value GitCommit -Force -Option AllScope

            function GitAdd { & git add --all $args }
            New-Alias -Name ga -Value GitAdd -Force -Option AllScope

            function GitTree { & git log --graph --oneline --decorate $args }
            New-Alias -Name gt -Value GitTree -Force -Option AllScope

            function GitPush { & git push $args }
            New-Alias -Name gps -Value GitPush -Force -Option AllScope

            function GitPull { & git pull $args }
            New-Alias -Name gpl -Value GitPull -Force -Option AllScope

            function GitFetch { & git fetch $args }
            New-Alias -Name gf -Value GitFetch -Force -Option AllScope

            function GitCheckout { & git checkout $args }
            New-Alias -Name gco -Value GitCheckout -Force -Option AllScope

            function GitBranch { & git branch $args }
            New-Alias -Name gb -Value GitBranch -Force -Option AllScope

            function GitRemote { & git remote -v $args }
            New-Alias -Name gr -Value GitRemote -Force -Option AllScope

            function GitDiff { & git diff $args }
            New-Alias -Name gd -Value GitDiff -Force -Option AllScope

            # Load the aliases immediately
            $null = Get-Alias | Where-Object { $_.Name -like "g*" }
            """;
    }
}
