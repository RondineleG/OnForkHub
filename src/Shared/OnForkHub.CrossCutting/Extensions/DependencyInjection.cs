// The .NET Foundation licenses this file to you under the MIT license.

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
}
