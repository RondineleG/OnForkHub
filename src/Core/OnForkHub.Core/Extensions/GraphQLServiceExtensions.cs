// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Core.GraphQL;

using System.Diagnostics.CodeAnalysis;

namespace OnForkHub.Core.Extensions;

[ExcludeFromCodeCoverage]
public static class GraphQLServiceExtensions
{
    public static IServiceCollection AddGraphQLOperations(this IServiceCollection services, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies)
        {
            var queryTypes = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsClass && typeof(IGraphQLQuery).IsAssignableFrom(t));
            foreach (var queryType in queryTypes)
            {
                services.AddTransient(typeof(IGraphQLQuery), queryType);
            }
            var mutationTypes = assembly.GetTypes().Where(t => !t.IsAbstract && t.IsClass && typeof(IGraphQLMutation).IsAssignableFrom(t));
            foreach (var mutationType in mutationTypes)
            {
                services.AddTransient(typeof(IGraphQLMutation), mutationType);
            }
        }
        return services;
    }

    public static IServiceCollection AddGraphQLServices(this IServiceCollection services)
    {
        services.AddSingleton<GraphQLEndpointManager>();
        return services;
    }
}
