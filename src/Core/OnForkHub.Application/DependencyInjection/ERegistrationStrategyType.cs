namespace OnForkHub.Application.DependencyInjection;

internal enum ERegistrationStrategyType
{
    AsImplementedInterfaces,

    AsSelf,

    AsSpecificTypes,

    UsingFactory,
}
