using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.CrossCutting.DependencyInjection;

public class RegistrationStrategy
{
    private readonly Type[] _typesToRegister;
    private readonly ServiceLifetime _lifetime;

    public RegistrationStrategy(Type[] typesToRegister, ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        _typesToRegister = typesToRegister ?? throw new ArgumentNullException(nameof(typesToRegister));
        _lifetime = lifetime;
    }

    public void Register(IServiceCollection services)
    {
        foreach (var type in _typesToRegister)
        {
            RegisterType(services, type);
        }
    }

    private void RegisterType(IServiceCollection services, Type type)
    {
        var interfaces = type.GetInterfaces();

        if (interfaces.Length > 0)
        {
            foreach (var interfaceType in interfaces)
            {
                services.Add(new ServiceDescriptor(interfaceType, type, _lifetime));
            }
        }
        else
        {
            services.Add(new ServiceDescriptor(type, type, _lifetime));
        }
    }
}
