using OnForkHub.Scripts.Git;
using OnForkHub.Scripts.Husky;

namespace OnForkHub.Scripts;

public static class Program
{
    private static async Task Main()
    {
        try
        {
            var projectRoot = GetProjectRootPath();
            Console.WriteLine($"[INFO] Project Root: {projectRoot}");

            if (!await GitFlowConfiguration.VerifyGitInstallationAsync())
            {
                Console.WriteLine("[ERROR] Git not installed.");
                return;
            }
            await GitFlowConfiguration.ApplySharedConfigurationsAsync();

            if (!await HuskyConfiguration.ConfigureHuskyAsync(projectRoot))
            {
                Console.WriteLine("[ERROR] Husky configuration failed.");
                return;
            }

            await PullRequestConfiguration.CreatePullRequestForGitFlowFinishAsync();

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
