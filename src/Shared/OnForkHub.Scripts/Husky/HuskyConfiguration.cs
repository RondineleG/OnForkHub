namespace OnForkHub.Scripts.Husky;

public static class HuskyConfiguration
{
    private static async Task ConfigureGitFlow(string projectRoot)
    {
        Console.WriteLine("[INFO] Configuring Git Flow...");

        try
        {
            if (!await RunProcessAsync("git", "rev-parse --git-dir", projectRoot))
            {
                await RunProcessAsync("git", "init", projectRoot);
                Console.WriteLine("[INFO] Git repository initialized");
            }

            var configs = new Dictionary<string, string>
            {
                { "gitflow.branch.master", "main" },
                { "gitflow.branch.develop", "dev" },
                { "gitflow.prefix.feature", "feature/" },
                { "gitflow.prefix.bugfix", "bugfix/" },
                { "gitflow.prefix.release", "release/" },
                { "gitflow.prefix.hotfix", "hotfix/" },
                { "gitflow.prefix.support", "support/" },
                { "gitflow.prefix.versiontag", "v" },
                { "gitflow.feature.finish", "false" },
                { "gitflow.feature.no-ff", "true" },
                { "gitflow.feature.no-merge", "true" },
                { "gitflow.feature.keepbranch", "true" },
            };

            foreach (var config in configs)
            {
                await RunProcessAsync("git", $"config --local {config.Key} {config.Value}", projectRoot);
            }

            if (!await BranchExists("main", projectRoot))
            {
                await RunProcessAsync("git", "checkout -b main", projectRoot);
                await RunProcessAsync("git", "add .", projectRoot);
                await RunProcessAsync("git", "commit --allow-empty -m \"Initial commit\"", projectRoot);
            }

            if (!await BranchExists("dev", projectRoot))
            {
                await RunProcessAsync("git", "checkout -b dev", projectRoot);
                await RunProcessAsync("git", "add .", projectRoot);
                await RunProcessAsync("git", "commit --allow-empty -m \"Initial dev commit\"", projectRoot);
            }

            await RunProcessAsync("git", "flow init -d -f", projectRoot);

            Console.WriteLine("[INFO] Git Flow configured successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception during Git Flow init: {ex.Message}");
            throw;
        }
    }

    private static async Task<bool> BranchExists(string branchName, string projectRoot)
    {
        try
        {
            var result = await RunProcessAsync("git", $"rev-parse --verify {branchName}", projectRoot);
            return result;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool> ConfigureHuskyAsync(string projectRoot)
    {
        Console.WriteLine("[INFO] Starting Husky configuration...");
        var huskyPath = Path.Combine(projectRoot, ".husky");
        Console.WriteLine($"[INFO] Husky Path: {huskyPath}");

        await ConfigureGitFlow(projectRoot);

        if (!await RunProcessAsync("dotnet", "tool restore", projectRoot))
        {
            Console.WriteLine("[ERROR] Failed to restore dotnet tools.");
            return false;
        }

        if (!await RunProcessAsync("dotnet", "husky install", projectRoot))
        {
            Console.WriteLine("[ERROR] Failed to install Husky.");
            return false;
        }

        try
        {
            var processInfo = new ProcessStartInfo("code", "-v")
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            using var process = Process.Start(processInfo);
            await process!.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                if (!await RunProcessAsync("git", "config --local core.editor \"code --wait\"", projectRoot))
                {
                    Console.WriteLine("[WARN] Failed to set VSCode as Git editor");
                }
                else
                {
                    Console.WriteLine("[INFO] VSCode configured as Git editor");
                }
            }
            else
            {
                if (!await RunProcessAsync("git", "config --local core.editor \"notepad\"", projectRoot))
                {
                    Console.WriteLine("[WARN] Failed to set Notepad as Git editor");
                }
                else
                {
                    Console.WriteLine("[INFO] Notepad configured as Git editor (VSCode not found)");
                }
            }
        }
        catch (Exception)
        {
            await RunProcessAsync("git", "config --local core.editor \"notepad\"", projectRoot);
            Console.WriteLine("[INFO] Notepad configured as Git editor (VSCode not found)");
        }

        Console.WriteLine("[INFO] Husky configured successfully.");
        return true;
    }

    private static async Task<bool> RunProcessAsync(string fileName, string arguments, string workingDirectory)
    {
        Console.WriteLine($"[INFO] Executing: {fileName} {arguments}");
        Console.WriteLine($"[INFO] Working directory: {workingDirectory}");

        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = workingDirectory,
        };

        try
        {
            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("[ERROR] Failed to start process.");
                return false;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            Console.WriteLine("[INFO] Process output:");
            Console.WriteLine(output);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine("[ERROR] Process error(s):");
                Console.WriteLine(error);
            }

            await process.WaitForExitAsync();
            if (process.ExitCode != 0)
            {
                Console.WriteLine($"[ERROR] Process exited with code {process.ExitCode}.");
                return false;
            }

            Console.WriteLine("[INFO] Process completed successfully.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception while executing process: {ex.Message}");
            return false;
        }
    }
}
