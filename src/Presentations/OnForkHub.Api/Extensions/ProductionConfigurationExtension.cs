using OnForkHub.CrossCutting.Authentication;
using OnForkHub.Persistence.Contexts;

namespace OnForkHub.Api.Extensions;

/// <summary>
/// Startup checks for the settings that must come from the environment in Production.
/// </summary>
[ExcludeFromCodeCoverage]
public static class ProductionConfigurationExtension
{
    private const int MinimumSecretKeyLength = 32;

    /// <summary>
    /// Stops the application when a Production secret is missing, too short or still a placeholder.
    /// Values are supplied through environment variables such as <c>Jwt__SecretKey</c>.
    /// </summary>
    public static void ValidateProductionConfiguration(this WebApplicationBuilder builder)
    {
        if (!builder.Environment.IsProduction())
        {
            return;
        }

        var configuration = builder.Configuration;
        var problems = new List<string>();

        var secretKey = configuration[$"{JwtOptions.SectionName}:{nameof(JwtOptions.SecretKey)}"];
        if (IsPlaceholder(secretKey) || secretKey!.Length < MinimumSecretKeyLength)
        {
            problems.Add($"Jwt:SecretKey must be set (Jwt__SecretKey) with at least {MinimumSecretKeyLength} characters.");
        }

        if (IsPlaceholder(configuration.GetConnectionString("DefaultConnection")))
        {
            problems.Add("ConnectionStrings:DefaultConnection must be set (ConnectionStrings__DefaultConnection).");
        }

        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
        if (origins.Length == 0 || origins.Any(IsPlaceholder))
        {
            problems.Add("Cors:AllowedOrigins must list the site origins (Cors__AllowedOrigins__0).");
        }

        if (problems.Count > 0)
        {
            throw new InvalidOperationException("Invalid Production configuration: " + string.Join(" ", problems));
        }
    }

    /// <summary>
    /// Applies pending EF Core migrations when <c>Database:MigrateOnStartup</c> is true.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task MigrateDatabaseIfRequestedAsync(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("Database:MigrateOnStartup"))
        {
            return;
        }

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<EntityFrameworkDataContext>();
        await context.Database.MigrateAsync();
    }

    private static bool IsPlaceholder(string? value) =>
        string.IsNullOrWhiteSpace(value) || value.Contains("${", StringComparison.Ordinal) || value.Contains("CHANGE_THIS", StringComparison.Ordinal);
}
