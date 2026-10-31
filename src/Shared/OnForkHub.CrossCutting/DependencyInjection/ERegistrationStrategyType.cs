namespace OnForkHub.CrossCutting.DependencyInjection;

public enum ERegistrationStrategyType
{
    AsImplementedInterfaces,

    AsSelf,

    AsSelfWithInterfaces,

    AsSpecificTypes,

    UsingFactory,
}
