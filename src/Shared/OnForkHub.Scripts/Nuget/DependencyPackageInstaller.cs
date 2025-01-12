namespace OnForkHub.Scripts.NuGet;

public class DependencyPackageInstaller(ILogger logger, IProcessRunner processRunner, string solutionRoot) : IPackageInstaller
{
    private const string DependenciesProjectPath = "src/Shared/OnForkHub.Dependencies/OnForkHub.Dependencies.csproj";

    private const string NugetSearchUrl = "https://api-v2v3search-0.nuget.org/query?q={0}&take=10";

    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    private readonly IProcessRunner _processRunner = processRunner ?? throw new ArgumentNullException(nameof(processRunner));

    private readonly string _solutionRoot = solutionRoot ?? throw new ArgumentNullException(nameof(solutionRoot));

    public async Task InstallPackageDirectAsync(string packageId, string version = "")
    {
        try
        {
            await InstallPackage(packageId, version);
        }
        catch (Exception ex)
        {
            _logger.Log(ELogLevel.Error, $"Failed to install {packageId}: {ex.Message}");
            throw;
        }
    }

    public async Task SearchAndInstallInteractiveAsync(string? searchTerm = null)
    {
        if (searchTerm == null)
        {
            _logger.Log(ELogLevel.Info, "Enter search term:");
            searchTerm = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return;
            }
        }

        var packages = await SearchPackages(searchTerm);
        if (packages.Count == 0)
        {
            _logger.Log(ELogLevel.Warning, "No packages found.");
            return;
        }

        await ProcessPackageSelections(packages);
    }

    private async Task InstallPackage(string packageName, string version)
    {
        var projectPath = Path.Combine(_solutionRoot, DependenciesProjectPath);
        if (!File.Exists(projectPath))
        {
            throw new FileNotFoundException($"Dependencies project not found: {projectPath}");
        }

        var versionArg = string.IsNullOrWhiteSpace(version) ? string.Empty : $"--version {version}";
        var command = $"add {projectPath} package {packageName} {versionArg}";
        _logger.Log(ELogLevel.Info, $"Installing {packageName} {version}...");
        await _processRunner.RunAsync("dotnet", command);
        _logger.Log(ELogLevel.Info, $"Installed {packageName}");
    }

    private async Task ProcessPackageSelections(List<PackageInfo> packages)
    {
        _logger.Log(ELogLevel.Info, "\nEnter selections (format: '0 4.*, 1 6.*'):");
        var input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            return;
        }

        foreach (var selection in input.Split(','))
        {
            var parts = selection.Trim().Split(' ', 2);
            if (!int.TryParse(parts[0], out var index) || index < 0 || index >= packages.Count)
            {
                _logger.Log(ELogLevel.Error, $"Invalid selection: {selection}");
                continue;
            }

            await InstallPackage(packages[index].Id, parts.Length > 1 ? parts[1] : string.Empty);
        }
    }

    private async Task<List<PackageInfo>> SearchPackages(string searchTerm)
    {
        using var client = new HttpClient();
        var url = $"{NugetSearchUrl}{searchTerm}";
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var results = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
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
            _logger.Log(ELogLevel.Info, $"{i}: {package.Id} ({package.Version})");
        }

        return packages;
    }
}
