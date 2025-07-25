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
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        services.AddEndpoin(typeof(Program));
        services.AddValidators(typeof(CategoryValidator).Assembly);
        services.AddEntityValidator(typeof(CategoryValidator).Assembly);
        services.AddUseCases(typeof(GetAllCategoryUseCase).Assembly);
        services.AddValidationRule(typeof(CategoryNameValidationRule).Assembly);

        services.AddScoped(typeof(IValidationBuilder<>), typeof(ValidationBuilder<>));
        services.AddScoped<IValidationBuilder<Category>, ValidationBuilder<Category>>();
        services.AddScoped<IEntityValidator<Category>, CategoryValidator>();
        services.AddScoped(typeof(IValidationService<>), typeof(ValidationService<>));
        services.AddScoped<ICategoryRepositoryEF, CategoryRepositoryEF>();
        services.AddScoped<ICategoryRepositoryRavenDB, CategoryRepositoryRavenDB>();
        services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();

        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IValidationService<Category>, CategoryValidationService>();

        return services;
    }

    public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
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

        services.AddSingleton<IDocumentStore>(serviceProvider =>
        {
            var store = new DocumentStore { Urls = ravenDbSettings?.Urls, Database = ravenDbSettings?.Database };
            store.Initialize();
            return store;
        });

        services.AddSingleton<RavenDbDataContext>();
        return services;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services, Assembly assembly)
    {
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IUseCase<,>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);
        
        strategy.Register(services);
        
        return services;
    }

    public static IServiceCollection AddValidationRule(this IServiceCollection services, Assembly assembly)
    {
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IValidationRule<>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);
        
        strategy.Register(services);
        
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services, Assembly assembly)
    {
        var scanner = new AssemblyScanner(assembly);
        var typeSelector = scanner.FindTypesImplementing(typeof(IEntityValidator<>));
        var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
        var strategy = typeSelector.CreateRegistrationStrategy(configurator);
        
        strategy.Register(services);

        return services;
    }

    public static bool DoesImplementInterfaceType(this Type type, Type interfaceType)
    {
        return !type.IsAbstract
            && type.IsClass
            && type.GetInterfaces().ToList().Exists(y => y.IsGenericType ? y.GetGenericTypeDefinition() == interfaceType : y == interfaceType);
    }

    public static void RegisterImplementationsOf<T>(
        this IServiceCollection services,
        Type markerType,
        ServiceLifetime lifetime = ServiceLifetime.Transient
    )
    {
        services.RegisterImplementationsOf(markerType, typeof(T), lifetime);
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
