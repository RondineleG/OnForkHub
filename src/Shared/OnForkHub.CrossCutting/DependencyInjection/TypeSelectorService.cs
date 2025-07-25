using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.CrossCutting.DependencyInjection;

/// <summary>
/// Service responsible for selecting types and creating registration strategies.
/// </summary>
public class TypeSelectorService
{
    private readonly IServiceCollection _services;

    /// <summary>
    /// Initializes a new instance with the service collection.
    /// </summary>
    public TypeSelectorService(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Creates a registration strategy for the specified types.
    /// </summary>
    public IRegistrationStrategy CreateStrategy()
    {
        var typesToRegister = GetTypesToRegister();
        return new RegistrationStrategy(typesToRegister, ServiceLifetime.Scoped);
    }

    /// <summary>
    /// Creates a registration strategy with custom lifetime.
    /// </summary>
    public IRegistrationStrategy CreateStrategy(ServiceLifetime lifetime)
    {
        var typesToRegister = GetTypesToRegister();
        return new RegistrationStrategy(typesToRegister, lifetime);
    }

    /// <summary>
    /// Creates a registration strategy with custom implementation resolver.
    /// </summary>
    public IRegistrationStrategy CreateStrategy(ServiceLifetime lifetime, Func<Type, Type> implementationResolver)
    {
        var typesToRegister = GetTypesToRegister();
        return new RegistrationStrategy(typesToRegister, lifetime, implementationResolver);
    }

    /// <summary>
    /// Gets the types to register based on the current service collection.
    /// </summary>
    private static List<Type> GetTypesToRegister()
    {
        // For now, return an empty list - this would be populated based on specific logic
        // In a real scenario, this might scan assemblies or filter existing services
        return new List<Type>();
    }

    /// <summary>
    /// Selects types based on a predicate.
    /// </summary>
    public IRegistrationStrategy CreateStrategy(Func<Type, bool> typeSelector, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        var allTypes = GetAllAvailableTypes();
        var selectedTypes = allTypes.Where(typeSelector).ToList();
        return new RegistrationStrategy(selectedTypes, lifetime);
    }

    /// <summary>
    /// Gets all available types for selection.
    /// </summary>
    private static IEnumerable<Type> GetAllAvailableTypes()
    {
        // This would typically scan assemblies or use reflection
        // For now, return empty collection
        return Enumerable.Empty<Type>();
    }
}
