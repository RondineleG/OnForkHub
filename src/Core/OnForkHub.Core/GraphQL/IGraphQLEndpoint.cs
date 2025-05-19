// The .NET Foundation licenses this file to you under the MIT license.

using HotChocolate.Types;

namespace OnForkHub.Core.GraphQL;

public abstract class MutationGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}

public abstract class QueryGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}

public interface IEndpointGraphQL
{
    void Register(IObjectTypeDescriptor descriptor);
}

public interface IGraphQLQueryHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}

public interface IGraphQLMutationHandler<TRequest, TResponse>
{
    Task<RequestResult<TResponse>> HandleAsync(TRequest input);
}
