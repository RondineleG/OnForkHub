using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.Core.GraphQL;

public abstract class QueryGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}
