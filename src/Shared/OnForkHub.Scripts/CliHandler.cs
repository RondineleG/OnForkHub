namespace OnForkHub.Scripts;

public class CliHandler(ILogger logger, IPackageInstaller packageInstaller)
{
    private readonly Dictionary<string, string> _commands =
        new()
        {
            { "-h", "Show this help" },
            { "-i <pkg> [-v version]", "Install package directly" },
            { "-s [term]", "Search and install packages" },
            { "-p", "Create pull request" },
        };

    public void ShowHelp()
    {
        logger.Log(ELogLevel.Info, "\nUsage: dtn <command> [options]");
        logger.Log(ELogLevel.Info, "\nCommands:");
        foreach (var cmd in _commands)
        {
            logger.Log(ELogLevel.Info, $"  {cmd.Key, -20} {cmd.Value}");
        }
        logger.Log(ELogLevel.Info, "\nExamples:");
        logger.Log(ELogLevel.Info, "  dtn -i Serilog -v 3.*");
        logger.Log(ELogLevel.Info, "  dtn -s Newtonsoft");
        logger.Log(ELogLevel.Info, "  dtn -p");
    }

    public async Task<bool> HandlePackageCommand(string[] args)
    {
        if (args.Contains("-h"))
        {
            ShowHelp();
            return true;
        }

        if (args.Contains("-i"))
        {
            var pkgIndex = Array.IndexOf(args, "-i") + 1;
            if (pkgIndex >= args.Length)
            {
                logger.Log(ELogLevel.Error, "Package name required");
                return false;
            }

            var version = string.Empty;
            if (args.Contains("-v"))
            {
                var vIndex = Array.IndexOf(args, "-v") + 1;
                if (vIndex < args.Length)
                {
                    version = args[vIndex];
                }
            }

            await packageInstaller.InstallPackageDirectAsync(args[pkgIndex], version);
            return true;
        }

        if (args.Contains("-s"))
        {
            var term = args.Length > Array.IndexOf(args, "-s") + 1 ? args[Array.IndexOf(args, "-s") + 1] : null;
            await packageInstaller.SearchAndInstallInteractiveAsync(term);
            return true;
        }

        return false;
    }
}
