namespace OnForkHub.Scripts;

public static class Program
{
    private static readonly ConsoleLogger StartupLogger = new();

    public static async Task<int> Main(string[] args)
    {
        try
        {
            using var host = CreateHostBuilder(args).Build();
            var app = ActivatorUtilities.CreateInstance<Startup>(host.Services);
            return await app.RunAsync(args);
        }
        catch (Exception ex)
        {
            StartupLogger.Log(ELogLevel.Error, $"Fatal error: {ex.Message}");
            StartupLogger.Log(ELogLevel.Debug, $"Stack Trace: {ex.StackTrace}");
            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureServices(
                (context, services) =>
                {
                    services.AddSingleton<ILogger, ConsoleLogger>();
                    services.AddSingleton<IProcessRunner, ProcessRunner>();
                    services.AddSingleton<IGitEditorService, GitEditorService>();
                    services.AddSingleton<IGitHubClient, GitHubClient>();
                    services.AddSingleton<IGitAliasConfiguration, GitAliasConfiguration>();

                    var projectRoot = GetProjectRootPath();
                    services.AddSingleton(projectRoot);

                    services.AddSingleton<GitFlowConfiguration>();
                    services.AddSingleton<HuskyConfiguration>();
                    services.AddSingleton<GitFlowPullRequestConfiguration>();
                    services.AddSingleton<Startup>();
                }
            );
    }

    private static string GetProjectRootPath()
    {
        var currentDir = new DirectoryInfo(Environment.CurrentDirectory);
        while (currentDir != null)
        {
            if (IsProjectRoot(currentDir))
            {
                StartupLogger.Log(ELogLevel.Info, $"Project root found: {currentDir.FullName}");
                return currentDir.FullName;
            }
            currentDir = currentDir.Parent;
        }

        throw new DirectoryNotFoundException("Could not find project root. Make sure a .sln file, .git folder, or .gitignore file exists.");
    }

    private static bool IsProjectRoot(DirectoryInfo directory)
    {
        return Directory.Exists(Path.Combine(directory.FullName, ".git"))
            || File.Exists(Path.Combine(directory.FullName, ".gitignore"))
            || directory.EnumerateFiles("*.sln").Any();
    }
}
