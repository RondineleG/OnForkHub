namespace OnForkHub.Scripts.Nuget;

public class DependencyPackageInstaller(ILogger logger, IProcessRunner processRunner, string solutionRoot) : IPackageInstaller
{
    private const string DependenciesProjectPath = "src/Shared/OnForkHub.Dependencies/OnForkHub.Dependencies.csproj";
    private static readonly CompositeFormat SearchUrlFormat = CompositeFormat.Parse("https://api-v2v3search-0.nuget.org/query?q={0}&take=10");

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));
    private readonly string _solutionRoot = solutionRoot ?? throw new ArgumentNullException(nameof(solutionRoot));

    public async Task InstallPackagesInteractively()
    {
        while (true)
        {
            _logger.Log(ELogLevel.Info, "\nEnter the NuGet package name to search, or 'exit' to quit:");
            var searchTerm = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(searchTerm) || searchTerm.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }

            await SearchAndInstallPackages(searchTerm);

            _logger.Log(ELogLevel.Info, "\nContinue searching and installing packages? (yes/no)");
            var continueChoice = Console.ReadLine();
            if (continueChoice?.ToLowerInvariant() != "yes")
            {
                break;
            }
        }
    }

    private async Task InstallPackage(string packageName, string version)
    {
        try
        {
            var projectPath = Path.Combine(_solutionRoot, DependenciesProjectPath);
            if (!File.Exists(projectPath))
            {
                throw new FileNotFoundException($"Dependencies project not found at: {projectPath}");
            }

            var versionArg = string.IsNullOrWhiteSpace(version) ? string.Empty : $"--version {version}";
            var command = $"add {projectPath} package {packageName} {versionArg}";

            _logger.Log(ELogLevel.Info, $"Installing {packageName} {version} to {projectPath}");
            await _processRunner.RunAsync("dotnet", command);
            _logger.Log(ELogLevel.Info, $"Successfully installed {packageName}");
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to install package {packageName}: {ex.Message}");
        }
    }

    private async Task SearchAndInstallPackages(string searchTerm)
    {
        try
        {
            var packages = await SearchPackages(searchTerm);
            if (packages.Count != 0)
            {
                await SelectAndInstallPackages(packages);
            }
            else
            {
                _logger.Log(ELogLevel.Warning, "No packages found.");
            }
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Error during package search and install: {ex.Message}");
        }
    }

    private async Task<List<PackageInfo>> SearchPackages(string searchTerm)
    {
        var apiUrl = string.Format(CultureInfo.InvariantCulture, SearchUrlFormat, searchTerm);
        using var client = new HttpClient();
        var response = await client.GetAsync(apiUrl);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var results = JsonDocument.Parse(json);
        var packages = new List<PackageInfo>();
        var data = results.RootElement.GetProperty("data");

        _logger.Log(ELogLevel.Info, "\nFound packages:");

        for (var i = 0; i < data.GetArrayLength(); i++)
        {
            var package = new PackageInfo(
                data[i].GetProperty("id").GetString() ?? string.Empty,
                data[i].GetProperty("version").GetString() ?? string.Empty,
                data[i].GetProperty("description").GetString() ?? string.Empty
            );
            packages.Add(package);
            _logger.Log(ELogLevel.Info, $"{i}: {package.Id} (Latest: {package.Version})");
        }

        return packages;
    }

    private async Task SelectAndInstallPackages(List<PackageInfo> packages)
    {
        _logger.Log(ELogLevel.Info, "\nEnter the package numbers to install, separated by commas (optionally specify version after space):");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        var selections = input.Split(',');
        foreach (var selection in selections)
        {
            var packageParts = selection.Trim().Split(' ');
            if (!int.TryParse(packageParts[0], out var packageIndex) || packageIndex < 0 || packageIndex >= packages.Count)
            {
                _logger.Log(ELogLevel.Error, $"Invalid selection: {selection}");
                continue;
            }

            var package = packages[packageIndex];
            var version = packageParts.Length > 1 ? packageParts[1] : string.Empty;
            await InstallPackage(package.Id, version);
        }
    }
}
