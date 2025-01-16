namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class RegistrationServicesExtensions
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
}
