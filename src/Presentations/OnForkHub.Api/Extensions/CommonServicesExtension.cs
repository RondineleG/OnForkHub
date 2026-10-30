using OnForkHub.Api.Services;
using OnForkHub.Core.Interfaces.Validations;
using OnForkHub.Core.Validations;
using OnForkHub.CrossCutting.DependencyInjection;
using OnForkHub.CrossCutting.Interfaces;
using OnForkHub.CrossCutting.Storage;
using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;

namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class CommonServicesExtension
{
    private const string RepositoryInterfacesNamespace = "OnForkHub.Core.Interfaces.Repositories";

    private const string ServiceInterfacesNamespace = "OnForkHub.Core.Interfaces.Services";

    public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddConventionServices();
        services.AddEntityFrameworkServices(configuration);
        services.AddFileStorageServices(configuration);
        services.AddExplicitServices();
        return services;
    }

    public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection String 'DefaultConnection' has not been found or is empty.");

        services.AddDbContext<EntityFrameworkDataContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IEntityFrameworkDataContext, EntityFrameworkDataContext>();

        return services;
    }

    /// <summary>
    /// Registers everything that follows a project convention, so new endpoints, use cases, validators,
    /// services and repositories need no manual registration.
    /// </summary>
    private static IServiceCollection AddConventionServices(this IServiceCollection services)
    {
        var apiAssembly = typeof(CommonServicesExtension).Assembly;
        var coreAssembly = typeof(IEntityValidator<>).Assembly;
        var applicationAssembly = typeof(GetAllCategoryUseCase).Assembly;
        var persistenceAssembly = typeof(EntityFrameworkDataContext).Assembly;

        services.Scan(scan => scan.FromAssemblies(apiAssembly).AddClassesImplementing<IEndpointAsync>().As<IEndpointAsync>().WithScopedLifetime());

        services.Scan(scan =>
            scan.FromAssemblies(coreAssembly)
                .AddClasses(type =>
                    ImplementsOpenGeneric(type, typeof(IEntityValidator<>))
                    || ImplementsOpenGeneric(type, typeof(IValidationRule<>))
                    || ImplementsOpenGeneric(type, typeof(IValidationService<>))
                )
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        services.Scan(scan =>
            scan.FromAssemblies(applicationAssembly).AddClassesImplementing(typeof(IUseCase<,>)).AsSelfWithInterfaces().WithScopedLifetime()
        );

        // Any class implementing a contract from Core.Interfaces.Services / Core.Interfaces.Repositories.
        services.Scan(scan =>
            scan.FromAssemblies(applicationAssembly, persistenceAssembly)
                .AddClasses(ImplementsServiceOrRepositoryContract)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
        );

        return services;
    }

    /// <summary>
    /// Registrations that do not fit a convention: open generics, lifetimes other than scoped and types without a contract.
    /// </summary>
    private static IServiceCollection AddExplicitServices(this IServiceCollection services)
    {
        services.AddScoped(typeof(IValidationBuilder<>), typeof(ValidationBuilder<>));
        services.AddScoped<IValidationService<Video>, ValidationService<Video>>();
        services.AddSingleton<DashManifestGenerator>();
        services.AddScoped<RealTimeNotificationService>();
        services.AddHostedService<VideoProcessingBackgroundService>();
        return services;
    }

    private static IServiceCollection AddFileStorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));

        var storageProvider = configuration.GetValue<string>("FileStorage:Provider") ?? "Local";
        if (storageProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
        {
            services.AddScoped<IFileStorageService, AzureBlobStorageService>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
        }

        return services;
    }

    private static bool ImplementsOpenGeneric(Type type, Type openGenericInterface) =>
        type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);

    private static bool ImplementsServiceOrRepositoryContract(Type type) =>
        type.GetInterfaces()
            .Any(i =>
                i.Namespace is not null
                && (
                    i.Namespace.StartsWith(ServiceInterfacesNamespace, StringComparison.Ordinal)
                    || i.Namespace.StartsWith(RepositoryInterfacesNamespace, StringComparison.Ordinal)
                )
            );
}
