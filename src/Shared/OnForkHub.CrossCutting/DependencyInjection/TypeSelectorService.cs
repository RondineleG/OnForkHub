namespace OnForkHub.CrossCutting.DependencyInjection;

public class TypeSelectorService
{
    public TypeSelectorService(Type[] typesToRegister)
    {
        _typesToRegister = typesToRegister ?? throw new ArgumentNullException(nameof(typesToRegister));
    }

    private readonly Type[] _typesToRegister;

    public RegistrationStrategy CreateRegistrationStrategy(ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        return new RegistrationStrategy(_typesToRegister, lifetime);
    }

    public RegistrationStrategy CreateRegistrationStrategy(LifetimeConfigurator configurator)
    {
        return new RegistrationStrategy(_typesToRegister, configurator.GetLifetime());
    }
}
