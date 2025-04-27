namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetEndpoint : IGraphQLServiceEndpoint
{
    public string Path => "/graphql-net";

    public IGraphQLServiceConfigurator Configurator { get; }

    public GraphQLNetEndpoint()
    {
        Configurator = new GraphQLNetConfigurator();
    }
}