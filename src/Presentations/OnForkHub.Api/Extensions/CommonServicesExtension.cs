using Microsoft.EntityFrameworkCore;

using OnForkHub.CrossCutting.DependencyInjection;
using OnForkHub.Persistence.Configurations;
using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Repositories;

using Raven.Client.Documents;

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

    public static IServiceCollection AddEntityValidator(this IServiceCollection services, Assembly assembly)
    {
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IEntityValidator<>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);

        strategy.Register(services);

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
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IUseCase<,>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);

        strategy.Register(services);

        return services;
    }

    private static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IValidationRule<>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);

        strategy.Register(services);

        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IEntityValidator<>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);

        strategy.Register(services);

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

    public static void RegisterImplementationsOf(
        this IServiceCollection services,
        Type markerType,
        Type interfaceType,
        ServiceLifetime lifetime = ServiceLifetime.Transient
    )
    {
        var scanner = new AssemblyScanner(markerType.Assembly);
        var typeSelector = scanner.FindTypesImplementing(interfaceType);
        var configurator = new LifetimeConfigurator(lifetime);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);

        strategy.Register(services);
    }
}
