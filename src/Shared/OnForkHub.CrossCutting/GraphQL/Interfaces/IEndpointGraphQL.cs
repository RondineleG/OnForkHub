namespace OnForkHub.CrossCutting.GraphQL.Interfaces;

public interface IEndpointGraphQL
{
    void Register(IObjectTypeDescriptor descriptor);
}
