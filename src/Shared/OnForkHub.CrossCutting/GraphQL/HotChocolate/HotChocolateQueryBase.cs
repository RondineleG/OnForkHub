using HotChocolate.Types;
using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public abstract class HotChocolateQueryBase : IGraphQLQuery
{
    public abstract string Description { get; }

    public abstract string Name { get; }

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
