using OnForkHub.CrossCutting.GraphQL;

namespace OnForkHub.CrossCutting.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphQLFromCrossCutting(this IServiceCollection services)
    {
        services.AddSingleton<IGraphQLConfigurator, HotChocolateConfigurator>();
        var configurator = new HotChocolateConfigurator();
        configurator.RegisterGraphQLServices(services);
        return services;
    }
}
