using HotChocolate.Types;

namespace OnForkHub.Core.GraphQL;

public interface IEndpointGraphQL
{
    void Register(IObjectTypeDescriptor descriptor);
}

public abstract class MutationGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}

public abstract class QueryGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}

public interface IGraphQLQueryHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}

// Interface genérica para Mutations
public interface IGraphQLMutationHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}
