namespace OnForkHub.Application.DependencyInjection;

internal enum RegistrationStrategyType
{
    AsImplementedInterfaces,

    AsSelf,

    AsSpecificTypes,

    UsingFactory,
}
