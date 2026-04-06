namespace OnForkHub.Core.Interfaces.DependencyInjection;

public interface ILifetimeConfigurator
{
    IRegistrationResult TryAddEnumerableScoped();

    IRegistrationResult TryAddEnumerableSingleton();

    IRegistrationResult TryAddEnumerableTransient();

    IRegistrationResult TryAddScoped();

    IRegistrationResult TryAddSingleton();

    IRegistrationResult TryAddTransient();

    IRegistrationResult WithScopedLifetime();

    IRegistrationResult WithSingletonLifetime();

    IRegistrationResult WithTransientLifetime();
}
