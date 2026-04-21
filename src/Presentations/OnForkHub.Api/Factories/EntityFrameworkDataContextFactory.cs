namespace OnForkHub.Api.Factories;

using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using OnForkHub.Persistence.Contexts;

/// <summary>
/// Factory for creating EntityFrameworkDataContext at design time (for migrations).
/// </summary>
public class EntityFrameworkDataContextFactory : IDesignTimeDbContextFactory<EntityFrameworkDataContext>
{
    /// <inheritdoc/>
    public EntityFrameworkDataContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();

        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=OnForkHub;Trusted_Connection=True;TrustServerCertificate=True;";

        var optionsBuilder = new DbContextOptionsBuilder<EntityFrameworkDataContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new EntityFrameworkDataContext(optionsBuilder.Options);
    }

    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .Build();
    }
}
