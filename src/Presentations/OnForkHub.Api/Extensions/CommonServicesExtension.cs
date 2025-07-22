namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class CommonServicesExtension
{
    private static readonly Action<ILogger, Exception, string> LogRegisterError =
        (Action<ILogger, Exception, string>)
            LoggerMessage.Define<string>(LogLevel.Error, new EventId(1, nameof(RegisterServices)), "Error when registering {ServiceName}");

    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddEndpoints();
        services.AddValidators();
        services.AddUseCases();
        services.AddValidationRules();
        services.AddApplicationServices();
        services.AddRepositories();
        services.AddValidationServices();
        services.AddSpecificServices();

        return services;
    }

    public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection String 'DefaultConnection' has not been found or is empty.");
        }

        services.AddDbContext<EntityFrameworkDataContext>(options => options.UseSqlServer(connectionString));

        services.AddScoped<IEntityFrameworkDataContext, EntityFrameworkDataContext>();

        return services;
    }

    public static IServiceCollection AddRavenDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        var ravenDbSettings = configuration.GetSection("RavenDbSettings").Get<RavenDbSettings>();

        if (ravenDbSettings is null)
        {
            throw new InvalidOperationException("Raven DB settings were not found in the 'RavenDbSettings' section.");
        }

        if (ravenDbSettings is null || ravenDbSettings.Urls == null || ravenDbSettings.Urls.Length == 0)
        {
            throw new InvalidOperationException("Raven DB URLs were not configured.");
        }

        if (string.IsNullOrWhiteSpace(ravenDbSettings.Database))
        {
            throw new InvalidOperationException("Raven DB Database Name has not been configured.");
        }

        services.AddSingleton<IDocumentStore>(serviceProvider =>
        {
            var store = new DocumentStore { Urls = ravenDbSettings.Urls!, Database = ravenDbSettings.Database! };

            store.Initialize();
            return store;
        });

        services.AddSingleton<RavenDbDataContext>();

        return services;
    }

    public static IServiceCollection RegisterServices(this IServiceCollection services, string serviceName, Action<IAssemblyScanner> scanAction)
    {
        try
        {
            services.Scan(scanAction);
        }
        catch (Exception ex)
        {
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetService<ILogger<object>>();

            if (logger != null)
            {
                LogRegisterError(logger, ex, serviceName);
            }
            else
            {
                Console.WriteLine($"Error when registering {serviceName}: {ex.Message}");
            }
        }

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        return services.RegisterServices(
            "services",
            scan => scan.FromAssemblyOf<BaseService>().AddClassesEndingWith("Service").AsImplementedInterfaces().WithScopedLifetime()
        );
    }

    private static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        return services.RegisterServices(
            "endpoints",
            scan => scan.FromAssemblyOf<CreateEndpoint>().AddClassesEndingWith("Endpoint").AsImplementedInterfaces().WithScopedLifetime()
        );
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        return services.RegisterServices(
            "repositórios",
            scan =>
                scan.FromAssemblyOf<EntityFrameworkDataContext>()
                    .AddClassesEndingWith("Repository", "RepositoryEF", "RepositoryRavenDB")
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
        );
    }

    private static IServiceCollection AddSpecificServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();
        return services;
    }

    private static IServiceCollection AddUseCases(this IServiceCollection services)
    {
        return services.RegisterServices(
            "use cases",
            scan =>
                scan.FromAssemblyOf<GetAllCategoryUseCase>()
                    .AddClassesImplementing(typeof(IUseCase<,>))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
        );
    }

    private static IServiceCollection AddValidationRules(this IServiceCollection services)
    {
        return services.RegisterServices(
            "validation rules",
            scan =>
                scan.FromAssemblyOf<CategoryNameValidationRule>()
                    .AddClassesImplementing(typeof(IValidationRule<>))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
        );
    }

    private static IServiceCollection AddValidationServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IValidationBuilder<>), typeof(ValidationBuilder<>));
        services.AddScoped(typeof(IValidationService<>), typeof(ValidationService<>));
        return services;
    }

    private static IServiceCollection AddValidators(this IServiceCollection services)
    {
        return services.RegisterServices(
            "validators",
            scan =>
                scan.FromAssemblyOf<CategoryValidator>()
                    .AddClassesImplementing(typeof(IEntityValidator<>))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
        );
    }
}
