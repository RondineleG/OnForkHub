namespace OnForkHub.Scripts;

public class Startup(ILogger logger, GitFlowConfiguration gitFlow, GitFlowPullRequestConfiguration prConfig, ICliHandler cliHandler)
{
    public async Task<int> RunAsync(string[] args)
    {
        try
        {
            if (args.Contains("-h"))
            {
                cliHandler.ShowHelp();
                return 0;
            }

            if (!await gitFlow.VerifyGitInstallationAsync())
            {
                logger.Log(ELogLevel.Error, "Git not installed");
                return 1;
            }

            await gitFlow.EnsureGitFlowConfiguredAsync();
            if (await cliHandler.HandlePackageCommand(args))
            {
                return 0;
            }

            if (args.Contains("-p") || args.Contains("pr-create"))
            {
                await gitFlow.EnsureCleanWorkingTreeAsync();
                await prConfig.CreatePullRequestForGitFlowFinishAsync();
                return 0;
            }

            logger.Log(ELogLevel.Info, "Display available commands and examples. run dtn -h for help.");
            return 1;
        }
        catch (Exception ex)
        {
            logger.Log(ELogLevel.Error, ex.Message);
            logger.Log(ELogLevel.Debug, ex.StackTrace ?? string.Empty);
            return 1;
        }
    }
}