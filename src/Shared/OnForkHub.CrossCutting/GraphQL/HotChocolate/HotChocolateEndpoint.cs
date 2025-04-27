namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateEndpoint : IGraphQLServiceEndpoint
{
    public string Path => "/graphql-hotchocolate";

    public IGraphQLServiceConfigurator Configurator { get; }

    public HotChocolateEndpoint()
    {
        Configurator = new HotChocolateConfigurator();
    }
}