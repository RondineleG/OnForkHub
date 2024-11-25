namespace OnForkHub.Scripts.Husky;

public static class HuskyConfiguration
{
    private static async Task ConfigureGitFlow(string projectRoot)
    {
        Console.WriteLine("[INFO] Configuring Git Flow...");

        var processInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = "flow init",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = projectRoot,
        };

        try
        {
            using var process = Process.Start(processInfo);
            if (process == null)
            {
                Console.WriteLine("[ERROR] Failed to start Git Flow init.");
                return;
            }

            using var writer = process.StandardInput;
            await writer.WriteLineAsync("main");
            await writer.WriteLineAsync("dev");
            await writer.WriteLineAsync("feature/");
            await writer.WriteLineAsync("bugfix/");
            await writer.WriteLineAsync("release/");
            await writer.WriteLineAsync("hotfix/");
            await writer.WriteLineAsync("support/");
            await writer.WriteLineAsync("v");

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            Console.WriteLine("[INFO] Git Flow init output:");
            Console.WriteLine(output);

            if (!string.IsNullOrWhiteSpace(error))
            {
                Console.WriteLine("[ERROR] Git Flow init errors:");
                Console.WriteLine(error);
            }

            await process.WaitForExitAsync();

            if (process.ExitCode == 0)
            {
                await RunProcessAsync("git", "config --local gitflow.feature.finish false", projectRoot);
                await RunProcessAsync("git", "config --local gitflow.feature.no-ff true", projectRoot);
                await RunProcessAsync("git", "config --local gitflow.feature.no-merge true", projectRoot);
                await RunProcessAsync("git", "config --local gitflow.feature.keepbranch true", projectRoot);

                Console.WriteLine("[INFO] Git Flow configured successfully");
            }
            else
            {
                Console.WriteLine("[ERROR] Git Flow initialization failed");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] Exception during Git Flow init: {ex.Message}");
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
