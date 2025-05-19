// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetEndpoint : IGraphQLEndpoint
{
    public GraphQLNetEndpoint()
    {
        Configurator = new GraphQLNetConfigurator();
    }

    public IGraphQLConfigurator Configurator { get; }

    public string Path => "/graphql-net";
}
