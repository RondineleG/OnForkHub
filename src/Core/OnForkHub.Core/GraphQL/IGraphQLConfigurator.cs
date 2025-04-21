using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.Core.GraphQL;

public interface IGraphQLOperation
{
    string Name { get; }
    string Description { get; }

    void Register(object descriptor);
}

public interface IGraphQLQuery : IGraphQLOperation
{
}

public interface IGraphQLMutation : IGraphQLOperation
{
}

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