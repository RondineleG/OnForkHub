using OnForkHub.Core.Interfaces.GraphQL;

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
