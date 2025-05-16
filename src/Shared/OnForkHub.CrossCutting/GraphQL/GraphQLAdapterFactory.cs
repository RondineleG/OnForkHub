using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.CrossCutting.GraphQL;

public class GraphQLAdapterFactory(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public IGraphQLMutation CreateGraphQLNetMutationAdapter<TRequest, TResponse>(string name, string description)
    {
        var handler = _serviceProvider.GetRequiredService<IGraphQLMutationHandler<TRequest, TResponse>>();
        return new GraphQLNetMutationAdapter<TRequest, TResponse>(handler, name, description);
    }

    public IGraphQLQuery CreateGraphQLNetQueryAdapter<TRequest, TResponse>(string name, string description)
    {
        var handler = _serviceProvider.GetRequiredService<IGraphQLQueryHandler<TRequest, TResponse>>();
        return new GraphQLNetQueryAdapter<TRequest, TResponse>(handler, name, description);
    }

    public IGraphQLMutation CreateHotChocolateMutationAdapter<TRequest, TResponse>(string name, string description)
    {
        var handler = _serviceProvider.GetRequiredService<IGraphQLMutationHandler<TRequest, TResponse>>();
        return new HotChocolateMutationAdapter<TRequest, TResponse>(handler, name, description);
    }

    public IGraphQLQuery CreateHotChocolateQueryAdapter<TRequest, TResponse>(string name, string description)
    {
        var handler = _serviceProvider.GetRequiredService<IGraphQLQueryHandler<TRequest, TResponse>>();
        return new HotChocolateQueryAdapter<TRequest, TResponse>(handler, name, description);
    }
}
