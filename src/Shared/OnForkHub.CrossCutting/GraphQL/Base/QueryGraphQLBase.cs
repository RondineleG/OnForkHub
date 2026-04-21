using OnForkHub.CrossCutting.GraphQL.Interfaces;

namespace OnForkHub.CrossCutting.GraphQL.Base;

public abstract class QueryGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}
