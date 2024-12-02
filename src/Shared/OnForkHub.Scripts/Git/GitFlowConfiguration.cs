using OnForkHub.Scripts.Enums;
using OnForkHub.Scripts.Interfaces;

namespace OnForkHub.Scripts.Git;

public sealed class GitFlowConfiguration(ILogger logger, IProcessRunner processRunner)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    public async Task<bool> VerifyGitInstallationAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Checking Git installation...");
            var gitVersion = await _processRunner.RunAsync("git", "--version");
            _logger.Log(ELogLevel.Info, $"Git Version: {gitVersion.Trim()}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "Failed to verify Git installation:");
            _logger.Log(ELogLevel.Error, ex.Message);
            return false;
        }
    }

    public async Task EnsureCleanWorkingTreeAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Checking for unstaged changes...");
            var statusOutput = await _processRunner.RunAsync("git", "status --porcelain");

            if (!string.IsNullOrWhiteSpace(statusOutput))
            {
                const string errorMessage = "Working tree contains unstaged changes. Please commit or stash changes before proceeding.";
                _logger.Log(ELogLevel.Error, errorMessage);
                throw new GitOperationException(errorMessage);
            }

            _logger.Log(ELogLevel.Info, "Working tree is clean.");
        }
        catch (Exception ex) when (ex is not GitOperationException)
        {
            _logger.Log(ELogLevel.Error, "Failed to verify clean working tree:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw new GitOperationException("Failed to verify working tree state.", ex);
        }
    }

    public async Task EnsureGitFlowConfiguredAsync()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Checking Git Flow configuration...");

            var tempScriptPath = Path.GetTempFileName();
            var isWindows = OperatingSystem.IsWindows();
            var scriptExtension = isWindows ? ".bat" : ".sh";
            var finalScriptPath = Path.ChangeExtension(tempScriptPath, scriptExtension);

            try
            {
                var scriptContent = isWindows ? GenerateWindowsScript() : GenerateUnixScript();

                await File.WriteAllTextAsync(finalScriptPath, scriptContent);

                if (!isWindows)
                {
                    await _processRunner.RunAsync("chmod", $"+x {finalScriptPath}");
                }

                await _processRunner.RunAsync(finalScriptPath, "");
                _logger.Log(ELogLevel.Info, "Git Flow initialized successfully.");

                await ConfigureAdditionalSettingsAsync();
            }
            finally
            {
                try
                {
                    if (File.Exists(finalScriptPath))
                    {
                        File.Delete(finalScriptPath);
                    }
                }
                catch
                {
                    _logger.Log(ELogLevel.Warning, "Could not delete temporary script file.");
                }
            }

            _logger.Log(ELogLevel.Info, "Git Flow configuration completed.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "An error occurred while configuring Git Flow:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw new GitOperationException("Failed to configure Git Flow.", ex);
        }
    }

    private static string GenerateWindowsScript()
    {
        return @"@echo off
git config --local gitflow.branch.master main
git config --local gitflow.branch.develop dev
git config --local gitflow.prefix.feature feature/
git config --local gitflow.prefix.bugfix bugfix/
git config --local gitflow.prefix.release release/
git config --local gitflow.prefix.hotfix hotfix/
git config --local gitflow.prefix.support support/
git config --local gitflow.prefix.versiontag v

git config --local core.bare false
git config --local core.logallrefupdates true

if not exist .git\refs\heads\dev (
    git checkout -b dev
    git push -u origin dev 2>nul
    git checkout main
)

echo ""GitFlow initialization complete.""
";
    }

    private static string GenerateUnixScript()
    {
        return @"#!/bin/bash
git config --local gitflow.branch.master main
git config --local gitflow.branch.develop dev
git config --local gitflow.prefix.feature feature/
git config --local gitflow.prefix.bugfix bugfix/
git config --local gitflow.prefix.release release/
git config --local gitflow.prefix.hotfix hotfix/
git config --local gitflow.prefix.support support/
git config --local gitflow.prefix.versiontag v

git config --local core.bare false
git config --local core.logallrefupdates true

if ! git show-ref --verify --quiet refs/heads/dev; then
    git checkout -b dev
    git push -u origin dev 2>/dev/null || true
    git checkout main
fi

echo 'GitFlow initialization complete.'
";
    }

    private async Task ConfigureAdditionalSettingsAsync()
    {
        var additionalConfigs = new Dictionary<string, string>
        {
            { "gitflow.feature.finish", "false" },
            { "gitflow.feature.no-ff", "true" },
            { "gitflow.feature.no-merge", "true" },
            { "gitflow.feature.keepbranch", "true" },
            { "gitflow.path.hooks", ".husky" },
        };

        foreach (var config in additionalConfigs)
        {
            try
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}");
                _logger.Log(ELogLevel.Info, $"Set {config.Key} to {config.Value}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to set {config.Key}: {ex.Message}");
            }
        }
    }
}
