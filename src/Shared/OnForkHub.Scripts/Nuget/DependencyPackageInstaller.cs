namespace OnForkHub.Scripts.NuGet;

public class DependencyPackageInstaller(ILogger logger, IProcessRunner processRunner, string solutionRoot) : IPackageInstaller
{
    private const string DirectoryPackagesPropsPath = "Directory.Packages.props";

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

    private static XmlElement? FindExistingPackageVersion(XmlElement itemGroup, string packageName)
    {
        foreach (XmlNode child in itemGroup.ChildNodes)
        {
            if (child is XmlElement element && element.Name == "PackageVersion" && element.GetAttribute("Include") == packageName)
            {
                return element;
            }
        }

        return null;
    }

    private static XmlElement FindOrCreatePackageVersionItemGroup(XmlDocument document, XmlElement projectElement)
    {
        // Look for existing ItemGroup with PackageVersion elements
        var itemGroups = projectElement.GetElementsByTagName("ItemGroup");
        foreach (XmlElement itemGroup in itemGroups)
        {
            if (itemGroup.HasChildNodes)
            {
                foreach (XmlNode child in itemGroup.ChildNodes)
                {
                    if (child is XmlElement element && element.Name == "PackageVersion")
                    {
                        return itemGroup;
                    }
                }
            }
        }

        // Create new ItemGroup if none found
        var newItemGroup = document.CreateElement("ItemGroup");
        projectElement.AppendChild(newItemGroup);
        return newItemGroup;
    }

    private static async Task<string> GetLatestPackageVersion(string packageName)
    {
        using var client = new HttpClient();
#pragma warning disable CA1863 // Use 'CompositeFormat' - not available in .NET 8
        var url = string.Format(CultureInfo.InvariantCulture, NugetSearchUrl, packageName);
#pragma warning restore CA1863
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var results = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var data = results.RootElement.GetProperty("data");

        if (data.GetArrayLength() > 0)
        {
            var firstResult = data[0];
            if (firstResult.TryGetProperty("version", out var versionElement))
            {
                return versionElement.GetString() ?? "1.0.0";
            }
        }

        return "1.0.0";
    }

    private async Task InstallPackage(string packageName, string version)
    {
        var directoryPackagesPath = Path.Combine(_solutionRoot, DirectoryPackagesPropsPath);
        if (!File.Exists(directoryPackagesPath))
        {
            throw new FileNotFoundException($"Directory.Packages.props not found: {directoryPackagesPath}");
        }

        // Get the latest version if not specified
        if (string.IsNullOrWhiteSpace(version))
        {
            version = await GetLatestPackageVersion(packageName);
        }

        // Update Directory.Packages.props with the new package
        await UpdateDirectoryPackagesProps(directoryPackagesPath, packageName, version);

        _logger.Log(ELogLevel.Info, $"Added {packageName} {version} to Directory.Packages.props");

        // Restore packages to ensure they are available
        await _processRunner.RunAsync("dotnet", "restore");
        _logger.Log(ELogLevel.Info, $"Successfully installed {packageName} {version}");
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
#pragma warning disable CA1863 // Use 'CompositeFormat' - not available in .NET 8
        var url = string.Format(CultureInfo.InvariantCulture, NugetSearchUrl, searchTerm);
#pragma warning restore CA1863
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

    private async Task UpdateDirectoryPackagesProps(string filePath, string packageName, string version)
    {
        var document = new XmlDocument();
        document.Load(filePath);

        var projectElement = document.DocumentElement;
        if (projectElement?.Name != "Project")
        {
            throw new InvalidOperationException("Invalid Directory.Packages.props format: missing Project element");
        }

        // Find or create ItemGroup for PackageVersion
        var itemGroup = FindOrCreatePackageVersionItemGroup(document, projectElement);

        // Check if package already exists
        var existingPackage = FindExistingPackageVersion(itemGroup, packageName);
        if (existingPackage != null)
        {
            // Update existing package version
            existingPackage.SetAttribute("Version", version);
            _logger.Log(ELogLevel.Info, $"Updated {packageName} from {existingPackage.GetAttribute("Version")} to {version}");
        }
        else
        {
            // Add new package
            var packageVersionElement = document.CreateElement("PackageVersion");
            packageVersionElement.SetAttribute("Include", packageName);
            packageVersionElement.SetAttribute("Version", version);
            itemGroup.AppendChild(packageVersionElement);
            _logger.Log(ELogLevel.Info, $"Added new package {packageName} with version {version}");
        }

        // Save the file
        var settings = new XmlWriterSettings
        {
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
        };

        await using var writer = XmlWriter.Create(filePath, settings);
        document.Save(writer);
    }
}
