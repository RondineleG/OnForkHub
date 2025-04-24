using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.CrossCutting.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphQLFromCrossCutting(this IServiceCollection services)
    {
        services.AddSingleton<IGraphQLConfigurator, HotChocolateConfigurator>();
        services.AddSingleton<IGraphQLConfigurator, GraphQLNetConfigurator>();

        // Create and configure the endpoint manager
        var endpointManager = new GraphQLEndpointManager();
        endpointManager.RegisterEndpoint(new HotChocolateEndpoint());
        endpointManager.RegisterEndpoint(new GraphQLNetEndpoint());
        endpointManager.ConfigureAll(services);

        // Register the configured instance
        services.AddSingleton(endpointManager);

        return services;
    }
}
