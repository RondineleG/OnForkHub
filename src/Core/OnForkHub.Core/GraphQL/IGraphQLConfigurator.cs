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

public interface IGraphQLConfigurator
{
    void RegisterGraphQLServices(IServiceCollection services);
}

public interface IGraphQLEndpoint
{
    string Path { get; }
    IGraphQLConfigurator Configurator { get; }
}

public class GraphQLEndpointManager
{
    private readonly List<IGraphQLEndpoint> _endpoints = [];

    public void RegisterEndpoint(IGraphQLEndpoint endpoint)
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

    public IReadOnlyList<IGraphQLEndpoint> Endpoints => _endpoints.AsReadOnly();
}
