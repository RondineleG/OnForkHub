using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.CrossCutting.DependencyInjection;

/// <summary>
/// Defines a strategy for registering types in the dependency injection container.
/// </summary>
public interface IRegistrationStrategy
{
    void Register(IServiceCollection services);
}

/// <summary>
/// Implementation of registration strategy that handles type registration with specific lifetimes.
/// </summary>
public class RegistrationStrategy : IRegistrationStrategy
{
    private readonly List<Type> _typesToRegister;
    private readonly ServiceLifetime _defaultLifetime;
    private readonly Func<Type, Type>? _implementationResolver;

    /// <summary>
    /// Initializes a new instance with types to register and default lifetime.
    /// </summary>
    public RegistrationStrategy(List<Type> typesToRegister, ServiceLifetime defaultLifetime)
    {
        _typesToRegister = typesToRegister ?? throw new ArgumentNullException(nameof(typesToRegister));
        _defaultLifetime = defaultLifetime;
    }

    /// <summary>
    /// Initializes a new instance with types to register, default lifetime, and implementation resolver.
    /// </summary>
    public RegistrationStrategy(List<Type> typesToRegister, ServiceLifetime defaultLifetime, Func<Type, Type> implementationResolver)
    {
        _typesToRegister = typesToRegister ?? throw new ArgumentNullException(nameof(typesToRegister));
        _defaultLifetime = defaultLifetime;
        _implementationResolver = implementationResolver ?? throw new ArgumentNullException(nameof(implementationResolver));
    }

    /// <summary>
    /// Registers all types in the service collection.
    /// </summary>
    public void Register(IServiceCollection services)
    {
        foreach (var type in _typesToRegister)
        {
            var implementationType = _implementationResolver?.Invoke(type) ?? type;
            
            var serviceDescriptor = new ServiceDescriptor(type, implementationType, _defaultLifetime);
            services.Add(serviceDescriptor);
        }
    }
}