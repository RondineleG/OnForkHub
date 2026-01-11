namespace OnForkHub.Core.Interfaces.GraphQL;

public interface IEndpointGraphQL
{
    void Register(IObjectTypeDescriptor descriptor);
}
