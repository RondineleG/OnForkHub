using OnForkHub.CrossCutting.GraphQL.Interfaces;

namespace OnForkHub.CrossCutting.GraphQL.Base;

public abstract class MutationGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}
