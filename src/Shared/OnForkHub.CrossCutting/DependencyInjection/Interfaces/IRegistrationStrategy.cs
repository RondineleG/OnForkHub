namespace OnForkHub.CrossCutting.DependencyInjection;

public interface IRegistrationStrategy
{
    ILifetimeConfigurator As<TService>();

    ILifetimeConfigurator As(params Type[] serviceTypes);

    ILifetimeConfigurator AsImplementedInterfaces();

    ILifetimeConfigurator AsSelf();

    ILifetimeConfigurator AsSelfWithInterfaces();

    ILifetimeConfigurator UsingFactory<TService>(Func<IServiceProvider, TService> factory);
}
