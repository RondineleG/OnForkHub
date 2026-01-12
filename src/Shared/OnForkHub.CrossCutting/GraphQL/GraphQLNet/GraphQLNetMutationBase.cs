using GraphQL.Types;

using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public abstract class GraphQLNetMutationBase : IGraphQLMutation
{
    public abstract string Description { get; }

    public abstract string Name { get; }

    public void Register(object descriptor)
    {
        if (descriptor is ObjectGraphType typedDescriptor)
        {
            RegisterMutation(typedDescriptor);
        }
        else
        {
            throw new ArgumentException($"Expected descriptor of type {nameof(ObjectGraphType)}");
        }
    }

    protected abstract void RegisterMutation(ObjectGraphType descriptor);
}
