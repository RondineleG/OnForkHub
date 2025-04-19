namespace OnForkHub.CrossCutting.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphQLFromCrossCutting(this IServiceCollection services)
    {
        var provider = services.BuildServiceProvider();
        var configurator = provider.GetRequiredService<IGraphQLConfigurator>();
        configurator.ConfigureGraphQL(services);
        return services;
    }
}