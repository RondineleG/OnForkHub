using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.Core.GraphQL;

public interface IGraphQLOperation
{
    string Name { get; }
    string Description { get; }

    void Register(object descriptor);
}

public interface IGraphQLQuery : IGraphQLOperation { }

public interface IGraphQLMutation : IGraphQLOperation { }

public interface IGraphQLSchemaBuilder
{
    IGraphQLSchemaBuilder AddQuery(IGraphQLQuery query);
    IGraphQLSchemaBuilder AddMutation(IGraphQLMutation mutation);
    object Build();
}

public interface IGraphQLServiceConfigurator
{
    void RegisterGraphQLServices(IServiceCollection services);
}

public interface IGraphQLServiceEndpoint
{
    string Path { get; }
    IGraphQLServiceConfigurator Configurator { get; }
}

public class GraphQLServiceEndpointManager
{
    private readonly List<IGraphQLServiceEndpoint> _endpoints = [];

    public void RegisterEndpoint(IGraphQLServiceEndpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }

    public void ConfigureAll(IServiceCollection services)
    {
        foreach (var endpoint in _endpoints)
        {
            endpoint.Configurator.RegisterGraphQLServices(services);
        }
    }

    public IReadOnlyList<IGraphQLServiceEndpoint> Endpoints => _endpoints.AsReadOnly();
}
