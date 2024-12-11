namespace OnForkHub.Scripts;

public sealed class CliHandler : ICliHandler
{
    private const int CommandColumnWidth = 20;

    private readonly Dictionary<string, string> _commands =
        new()
        {
            { "-h", "Show this help" },
            { "-i <pkg> [-v version]", "Install package directly" },
            { "-s [term]", "Search and install packages" },
            { "-p", "Create pull request" },
        };

    private readonly ILogger _logger;
    private readonly IPackageInstaller _packageInstaller;

    public CliHandler(ILogger logger, IPackageInstaller packageInstaller)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _packageInstaller = packageInstaller ?? throw new ArgumentNullException(nameof(packageInstaller));
    }

    public void ShowHelp()
    {
        _logger.Log(ELogLevel.Info, "\nUsage: dtn <command> [options]");
        _logger.Log(ELogLevel.Info, "\nCommands:");

        foreach (var cmd in _commands)
        {
            var formattedCommand = $"  {cmd.Key.PadRight(CommandColumnWidth)} {cmd.Value}";
            _logger.Log(ELogLevel.Info, formattedCommand);
        }

        _logger.Log(ELogLevel.Info, "\nExamples:");
        _logger.Log(ELogLevel.Info, "  dtn -i Serilog -v 3.*");
        _logger.Log(ELogLevel.Info, "  dtn -s Newtonsoft");
        _logger.Log(ELogLevel.Info, "  dtn -p");
    }

    public async Task<bool> HandlePackageCommand(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            ShowHelp();
            return true;
        }

        if (args.Contains("-h"))
        {
            ShowHelp();
            return true;
        }

        if (args.Contains("-i"))
        {
            return await HandleDirectInstall(args);
        }

        if (args.Contains("-s"))
        {
            return await HandleSearch(args);
        }

        return false;
    }

    private async Task<bool> HandleDirectInstall(string[] args)
    {
        var pkgIndex = Array.IndexOf(args, "-i") + 1;
        if (pkgIndex >= args.Length)
        {
            _logger.Log(ELogLevel.Error, "Package name required");
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

        try
        {
            await _packageInstaller.InstallPackageDirectAsync(args[pkgIndex], version);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to install package: {ex.Message}");
            return false;
        }
    }

    private async Task<bool> HandleSearch(string[] args)
    {
        try
        {
            var searchIndex = Array.IndexOf(args, "-s");
            var term = args.Length > searchIndex + 1 ? args[searchIndex + 1] : null;
            await _packageInstaller.SearchAndInstallInteractiveAsync(term);
            return true;
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Search failed: {ex.Message}");
            return false;
        }
    }
}
