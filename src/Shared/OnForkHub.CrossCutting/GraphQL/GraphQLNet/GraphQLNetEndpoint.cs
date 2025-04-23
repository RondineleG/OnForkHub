namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetEndpoint : IGraphQLEndpoint
{
    public string Path => "/graphql-net";

    public IGraphQLConfigurator Configurator { get; }

    public GraphQLNetEndpoint()
    {
        Configurator = new GraphQLNetConfigurator();
    }
}
