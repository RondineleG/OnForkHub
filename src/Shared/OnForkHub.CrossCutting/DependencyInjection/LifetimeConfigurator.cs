using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.CrossCutting.DependencyInjection;

/// <summary>
/// Configures service lifetimes for dependency injection registration.
/// </summary>
public class LifetimeConfigurator
{
    public ServiceLifetime Lifetime { get; }
    public Type ServiceType { get; }
    public Type? ImplementationType { get; }
    public string? Name { get; }

    /// <summary>
    /// Initializes a new instance with service type, implementation type, and lifetime.
    /// </summary>
    public LifetimeConfigurator(Type serviceType, Type implementationType, ServiceLifetime lifetime)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        Lifetime = lifetime;
    }

    /// <summary>
    /// Initializes a new instance with service type, implementation type, lifetime, and name.
    /// </summary>
    public LifetimeConfigurator(Type serviceType, Type implementationType, ServiceLifetime lifetime, string name)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        Lifetime = lifetime;
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}