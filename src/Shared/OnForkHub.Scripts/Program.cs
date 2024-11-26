using OnForkHub.Scripts.Git;
using OnForkHub.Scripts.Husky;

namespace OnForkHub.Scripts;

public static class Program
{
    private static async Task Main(string[] args)
    {
        Console.WriteLine($"[INFO] args: {args}");
        try
        {
            var projectRoot = GetProjectRootPath();
            Console.WriteLine($"[INFO] Project Root: {projectRoot}");

            if (!await GitFlowConfiguration.VerifyGitInstallationAsync())
            {
                Console.WriteLine("[ERROR] Git not installed.");
                return;
            }

            if (await GitFlowConfiguration.VerifyGitInstallationAsync())
            {
                await GitFlowConfiguration.EnsureCleanWorkingTreeAsync();
                await GitFlowConfiguration.EnsureGitFlowConfiguredAsync();
                Console.WriteLine("[INFO] Git Flow configuration completed successfully.");
            }

            if (!await HuskyConfiguration.ConfigureHuskyAsync(projectRoot))
            {
                Console.WriteLine("[ERROR] Husky configuration failed.");
                return;
            }

            if (args.Contains("pr-create"))
            {
                try
                {
                    await GitFlowPullRequestConfiguration.CreatePullRequestForGitFlowFinishAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] An error occurred: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("[INFO] Skipping PR creation (pr-create flag not present)");
            }

            Console.WriteLine("[INFO] Configuration completed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] An unexpected error occurred: {ex.Message}");
            Console.WriteLine($"[DEBUG] Stack Trace: {ex.StackTrace}");
        }
    }

    private static string GetProjectRootPath()
    {
        var currentDir = new DirectoryInfo(Environment.CurrentDirectory);
        while (currentDir != null)
        {
            var gitPath = Path.Combine(currentDir.FullName, ".git");
            var gitIgnorePath = Path.Combine(currentDir.FullName, ".gitignore");

            if (Directory.Exists(gitPath) || File.Exists(gitIgnorePath) || currentDir.EnumerateFiles("*.sln").Any())
            {
                Console.WriteLine($"[INFO] Project root found: {currentDir.FullName}");
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }

        throw new DirectoryNotFoundException("[ERROR] Could not find project root. Make sure a .sln file, .git folder, or .gitignore file exists.");
    }
}
