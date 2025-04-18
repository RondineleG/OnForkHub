using OnForkHub.Application.Services;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Validations;
using OnForkHub.Persistence.Configurations;

using Raven.Client.Documents;

namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ServicesExtensionsConfiguration
{
    public static IServiceCollection AddEntityValidator(this IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && type.IsClass
                && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityValidator<>))
            );
        foreach (var type in validatorTypes)
        {
            var implementedInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityValidator<>));
            services.Add(new ServiceDescriptor(implementedInterface, type, ServiceLifetime.Scoped));
            Console.WriteLine($"Registered: {implementedInterface} -> {type}");
        }
        return services;
    }

    public static IServiceCollection AddUseCases(this IServiceCollection services, Assembly assembly)
    {
        var useCaseTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && type.IsClass
                && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUseCase<,>))
            );

        foreach (var type in useCaseTypes)
        {
            var implementedInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IUseCase<,>));
            services.Add(new ServiceDescriptor(implementedInterface, type, ServiceLifetime.Scoped));
            Console.WriteLine($"Registered UseCase: {type.Name} -> {implementedInterface}");
        }
        return services;
    }

    public static IServiceCollection AddValidationRule(this IServiceCollection services, Assembly assembly)
    {
        var useCaseTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && type.IsClass
                && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidationRule<>))
            );
        foreach (var type in useCaseTypes)
        {
            var implementedInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidationRule<>));
            services.Add(new ServiceDescriptor(implementedInterface, type, ServiceLifetime.Scoped));
        }
        return services;
    }

    public static IServiceCollection AddValidators(this IServiceCollection services, Assembly assembly)
    {
        var validatorTypes = assembly
            .GetTypes()
            .Where(type =>
                !type.IsAbstract
                && type.IsClass
                && type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityValidator<>))
            );

        foreach (var type in validatorTypes)
        {
            var implementedInterface = type.GetInterfaces().First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEntityValidator<>));
            services.AddScoped(implementedInterface, type);
        }

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
        markerType
            .Assembly.GetTypes()
            .Where(x => x.DoesImplementInterfaceType(interfaceType))
            .ForEach(x =>
                services.Add(
                    new ServiceDescriptor(
                        x.GetInterfaces().First(y => y.IsGenericType ? y.GetGenericTypeDefinition() == interfaceType : y == interfaceType),
                        x,
                        lifetime
                    )
                )
            );
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

    public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<EntityFrameworkDataContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IEntityFrameworkDataContext, EntityFrameworkDataContext>();
        return services;
    }

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

        return services;
    }
}