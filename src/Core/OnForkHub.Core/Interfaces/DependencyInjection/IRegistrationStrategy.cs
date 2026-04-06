namespace OnForkHub.Core.Interfaces.DependencyInjection;

public interface IRegistrationStrategy
{
    ILifetimeConfigurator As<TService>();

    ILifetimeConfigurator As(params Type[] serviceTypes);

    ILifetimeConfigurator AsImplementedInterfaces();

    ILifetimeConfigurator AsSelf();

    ILifetimeConfigurator UsingFactory<TService>(Func<IServiceProvider, TService> factory);
}
