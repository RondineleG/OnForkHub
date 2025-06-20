using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateEndpoint : IGraphQLEndpoint
{
    public HotChocolateEndpoint()
    {
        Configurator = new HotChocolateConfigurator();
    }

    public IGraphQLConfigurator Configurator { get; }

    public string Path => "/graphql-hotchocolate";
}
