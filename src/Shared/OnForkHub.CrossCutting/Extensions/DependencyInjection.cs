using OnForkHub.Core.Interfaces.GraphQL;
using OnForkHub.CrossCutting.DependencyInjection;
using OnForkHub.CrossCutting.GraphQL;
using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.CrossCutting.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphQLFromCrossCutting(this IServiceCollection services)
    {
        services.AddSingleton<IGraphQLConfigurator, HotChocolateConfigurator>();
        services.AddSingleton<IGraphQLConfigurator, GraphQLNetConfigurator>();

        var endpointManager = new GraphQLEndpointManager();
        endpointManager.RegisterEndpoint(new HotChocolateEndpoint());
        endpointManager.RegisterEndpoint(new GraphQLNetEndpoint());
        endpointManager.ConfigureAll(services);
        services.AddSingleton(endpointManager);

        services.AddSingleton<GraphQLAdapterFactory>();

        return services;
    }

    /// <summary>
    /// Adds the enhanced dependency injection components to the service collection.
    /// </summary>
    public static IServiceCollection AddEnhancedDependencyInjection(this IServiceCollection services)
    {
        // Register the TypeSelectorService as a singleton
        services.AddSingleton<TypeSelectorService>();

        // Create a TypeSelectorService instance to create strategies
        var typeSelectorService = new TypeSelectorService(services);

        // Create and register a default registration strategy
        var defaultStrategy = TypeSelectorService.CreateStrategy();
        services.AddSingleton<IRegistrationStrategy>(defaultStrategy);

        return services;
    }
}
