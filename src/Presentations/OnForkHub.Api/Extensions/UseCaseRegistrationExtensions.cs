using OnForkHub.Core.Interfaces.Validations;

namespace OnForkHub.Api.Extensions;

public static class UseCaseRegistrationExtensions
{
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
            Console.WriteLine($"Registrado: {implementedInterface} -> {type}");
        }
        return services;
    }
}