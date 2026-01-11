using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.Core.Interfaces.GraphQL;

public interface IGraphQLConfigurator
{
    void RegisterGraphQLServices(IServiceCollection services);
}

public interface IGraphQLEndpoint
{
    IGraphQLConfigurator Configurator { get; }

    string Path { get; }
}

public interface IGraphQLMutation : IGraphQLOperation { }

public interface IGraphQLOperation
{
    string Description { get; }

    string Name { get; }

    void Register(object descriptor);
}

public interface IGraphQLQuery : IGraphQLOperation { }

public interface IGraphQLSchemaBuilder
{
    IGraphQLSchemaBuilder AddMutation(IGraphQLMutation mutation);

    IGraphQLSchemaBuilder AddQuery(IGraphQLQuery query);

    object Build();
}

public class GraphQLEndpointManager
{
    private readonly List<IGraphQLEndpoint> _endpoints = [];

    public IReadOnlyList<IGraphQLEndpoint> Endpoints => _endpoints.AsReadOnly();

    public void ConfigureAll(IServiceCollection services)
    {
        foreach (var endpoint in _endpoints)
        {
            endpoint.Configurator.RegisterGraphQLServices(services);
        }
    }

    public void RegisterEndpoint(IGraphQLEndpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }
}
