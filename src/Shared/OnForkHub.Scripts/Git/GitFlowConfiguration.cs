namespace OnForkHub.Scripts.Git;

public sealed class GitFlowConfiguration(ILogger logger, IProcessRunner processRunner)
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    public async Task ConfigureGlobalAutoSetupRemote()
    {
        try
        {
            _logger.Log(ELogLevel.Info, "Configuring global Git autoSetupRemote...");
            await _processRunner.RunAsync("git", "config --global push.autoSetupRemote true");
            _logger.Log(ELogLevel.Info, "Global autoSetupRemote configured successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Warning, $"Failed to set global autoSetupRemote: {ex.Message}");
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

            var workingDir = FindGitRepositoryRoot(Environment.CurrentDirectory) ?? throw new GitOperationException("Não foi possível encontrar a raiz do repositório Git. Certifique-se de que você está em um repositório Git válido.");
            _logger.Log(ELogLevel.Debug, $"Working directory: {workingDir}");

            if (!Directory.Exists(Path.Combine(workingDir, ".git")))
            {
                _logger.Log(ELogLevel.Info, "Initializing Git repository...");
                await _processRunner.RunAsync("git", "init");
            }

            await RunGitFlowInit(workingDir);
            await ConfigureGitFlowSettings(workingDir);
            await EnsureRequiredBranchesExistAsync();
            await ConfigureGlobalAutoSetupRemote();

            _logger.Log(ELogLevel.Info, "Git Flow configuration completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, "An error occurred while configuring Git Flow:");
            _logger.Log(ELogLevel.Error, ex.Message);
            throw;
        }
    }

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

    private static string FindGitRepositoryRoot(string startPath)
    {
        var currentDirectory = new DirectoryInfo(startPath);
        while (currentDirectory != null)
        {
            if (currentDirectory.Name.Equals("OnForkHub", StringComparison.OrdinalIgnoreCase))
            {
                return currentDirectory.FullName;
            }
            currentDirectory = currentDirectory.Parent;
        }
        currentDirectory = new DirectoryInfo(startPath);
        while (currentDirectory != null)
        {
            if (Directory.Exists(Path.Combine(currentDirectory.FullName, ".git")))
            {
                return currentDirectory.FullName;
            }
            currentDirectory = currentDirectory.Parent;
        }
        return string.Empty;
    }

    private static bool IsFeatureBranch(string branchName)
    {
        return branchName.StartsWith("feature/", StringComparison.OrdinalIgnoreCase);
    }

    private async Task ConfigureGitFlowSettings(string workingDir)
    {
        _logger.Log(ELogLevel.Info, "Configuring Git Flow settings...");

        var configs = new Dictionary<string, string>
        {
            { "gitflow.branch.main", "main" },
            { "gitflow.branch.develop", "dev" },
            { "gitflow.prefix.feature", "feature/" },
            { "gitflow.prefix.bugfix", "bugfix/" },
            { "gitflow.prefix.release", "release/" },
            { "gitflow.prefix.hotfix", "hotfix/" },
            { "gitflow.prefix.support", "support/" },
            { "gitflow.prefix.versiontag", "v" },
            { "gitflow.feature.start.fetch", "true" },
            { "gitflow.feature.finish.fetch", "true" },
            { "gitflow.feature.finish", "false" },
            { "gitflow.feature.no-ff", "true" },
            { "gitflow.feature.no-merge", "true" },
            { "gitflow.feature.keepbranch", "true" },
            { "gitflow.path.hooks", ".husky" },
        };

        foreach (var config in configs)
        {
            try
            {
                var command = $"config --local {config.Key} {config.Value}";
                await _processRunner.RunAsync("git", command, workingDir);
                _logger.Log(ELogLevel.Debug, $"Set {config.Key} to {config.Value}");
            }
            catch (Exception ex)
            {
                _logger.Log(ELogLevel.Warning, $"Failed to set {config.Key}: {ex.Message}");
            }
        }

        await _processRunner.RunAsync("git", "config --local gitflow.initialized true", workingDir);
        _logger.Log(ELogLevel.Info, "Git Flow settings configured successfully.");
    }

    private async Task CreateBranch(string branchName)
    {
        try
        {
            _logger.Log(ELogLevel.Info, $"Creating {branchName} branch...");
            await _processRunner.RunAsync("git", $"branch {branchName}");

            try
            {
                await _processRunner.RunAsync("git", $"push -u origin {branchName}");
                _logger.Log(ELogLevel.Info, $"Pushed {branchName} branch to remote.");
            }
            catch
            {
                _logger.Log(ELogLevel.Warning, $"Could not push {branchName} branch to remote. This is normal for new repositories.");
            }
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error creating {branchName} branch: {ex.Message}");
            throw;
        }
    }

    private async Task EnsureRequiredBranchesExistAsync()
    {
        try
        {
            var currentBranch = await GetCurrentBranch();
            var existingBranches = await GetExistingBranches();

            if (!existingBranches.Contains("main") && !IsFeatureBranch(currentBranch))
            {
                await CreateBranch("main");
            }

            if (!existingBranches.Contains("dev") && !IsFeatureBranch(currentBranch))
            {
                await CreateBranch("dev");
            }

            _logger.Log(ELogLevel.Info, "Required branches verified.");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error ensuring required branches: {ex.Message}");
            throw;
        }
    }
    private async Task<string> GetCurrentBranch()
    {
        return (await _processRunner.RunAsync("git", "rev-parse --abbrev-ref HEAD")).Trim();
    }

    private async Task<List<string>> GetExistingBranches()
    {
        var branchOutput = await _processRunner.RunAsync("git", "branch");
        return [.. branchOutput.Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(b => b.Trim('*', ' '))];
    }

    private async Task RunGitFlowInit(string workingDir)
    {
        _logger.Log(ELogLevel.Info, "Initializing Git Flow...");

        try
        {
            await _processRunner.RunAsync("git", "flow init -f -d", workingDir);
            _logger.Log(ELogLevel.Info, "Initial Git Flow initialization done.");

            var branchConfigs = new Dictionary<string, string>
            {
                { "flow.branch.main", "main" },
                { "flow.branch.develop", "dev" },
                { "flow.prefix.feature", "feature/" },
                { "flow.prefix.bugfix", "bugfix/" },
                { "flow.prefix.release", "release/" },
                { "flow.prefix.hotfix", "hotfix/" },
                { "flow.prefix.support", "support/" },
                { "flow.prefix.versiontag", "v" },
            };

            foreach (var config in branchConfigs)
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}", workingDir);
                _logger.Log(ELogLevel.Debug, $"Set {config.Key} to {config.Value}");
            }

            var behaviorConfigs = new Dictionary<string, string>
            {
                { "flow.feature.start.fetch", "true" },
                { "flow.feature.finish.fetch", "true" },
                { "flow.feature.finish", "false" },
                { "flow.feature.no-ff", "true" },
                { "flow.feature.no-merge", "true" },
                { "flow.feature.keepbranch", "true" },
                { "flow.path.hooks", ".husky" },
                { "flow.initialized", "true" },
            };

            foreach (var config in behaviorConfigs)
            {
                await _processRunner.RunAsync("git", $"config --local {config.Key} {config.Value}", workingDir);
                _logger.Log(ELogLevel.Debug, $"Set {config.Key} to {config.Value}");
            }

            var initCheck = await _processRunner.RunAsync("git", "flow version", workingDir);
            _logger.Log(ELogLevel.Info, $"Git Flow initialized successfully. Version: {initCheck}");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Git Flow initialization failed: {ex.Message}");

            var startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = "flow init -f",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workingDir,
                CreateNoWindow = true,
            };

            using var process = new Process { StartInfo = startInfo };
            process.Start();

            using (var writer = process.StandardInput)
            {
                await writer.WriteLineAsync("main");
                await writer.WriteLineAsync("dev");
                await writer.WriteLineAsync("feature/");
                await writer.WriteLineAsync("bugfix/");
                await writer.WriteLineAsync("release/");
                await writer.WriteLineAsync("hotfix/");
                await writer.WriteLineAsync("support/");
                await writer.WriteLineAsync("v");
            }

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                _logger.Log(ELogLevel.Error, $"Alternative Git Flow initialization failed: {error}");
                throw new GitOperationException($"Git Flow initialization failed after multiple attempts: {error}");
            }

            _logger.Log(ELogLevel.Info, "Git Flow initialized successfully using alternative method.");
        }
    }
}
