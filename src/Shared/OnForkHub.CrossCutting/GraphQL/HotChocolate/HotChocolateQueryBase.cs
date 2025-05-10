using HotChocolate.Types;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public abstract class HotChocolateQueryBase : IGraphQLQuery
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public void Register(object descriptor)
    {
        if (descriptor is IObjectTypeDescriptor typedDescriptor)
        {
            RegisterQuery(typedDescriptor);
        }
        else
        {
            throw new ArgumentException($"Expected descriptor of type {nameof(IObjectTypeDescriptor)}");
        }
    }

    protected abstract void RegisterQuery(IObjectTypeDescriptor descriptor);
}
