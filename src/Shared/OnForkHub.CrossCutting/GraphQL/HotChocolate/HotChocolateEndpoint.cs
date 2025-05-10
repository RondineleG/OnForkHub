namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateEndpoint : IGraphQLEndpoint
{
    public string Path => "/graphql-hotchocolate";

    public IGraphQLConfigurator Configurator { get; }

    public HotChocolateEndpoint()
    {
        Configurator = new HotChocolateConfigurator();
    }
}