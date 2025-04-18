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