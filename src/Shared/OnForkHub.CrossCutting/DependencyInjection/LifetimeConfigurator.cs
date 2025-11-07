namespace OnForkHub.CrossCutting.DependencyInjection;

public class LifetimeConfigurator
{
    public LifetimeConfigurator(IServiceCollection services)
    {
        _lifetime = ServiceLifetime.Scoped;
    }

    public LifetimeConfigurator(List<Type> types)
    {
        _lifetime = ServiceLifetime.Scoped;
    }

    public LifetimeConfigurator(Type[] types)
    {
        _lifetime = ServiceLifetime.Scoped;
    }

    public LifetimeConfigurator(Func<Type, bool> predicate)
    {
        _lifetime = ServiceLifetime.Scoped;
    }

    public LifetimeConfigurator(ServiceLifetime lifetime)
    {
        _lifetime = lifetime;
    }

    private ServiceLifetime _lifetime;

    public LifetimeConfigurator AsScoped()
    {
        _lifetime = ServiceLifetime.Scoped;
        return this;
    }

    public LifetimeConfigurator AsSingleton()
    {
        _lifetime = ServiceLifetime.Singleton;
        return this;
    }

    public LifetimeConfigurator AsTransient()
    {
        _lifetime = ServiceLifetime.Transient;
        return this;
    }

    public ServiceLifetime GetLifetime()
    {
        return _lifetime;
    }
}
