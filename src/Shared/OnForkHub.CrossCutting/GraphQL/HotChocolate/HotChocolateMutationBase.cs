// The .NET Foundation licenses this file to you under the MIT license.

using HotChocolate.Types;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public abstract class HotChocolateMutationBase : IGraphQLMutation
{
    public abstract string Description { get; }

    public abstract string Name { get; }

    public void Register(object descriptor)
    {
        if (descriptor is IObjectTypeDescriptor typedDescriptor)
        {
            RegisterMutation(typedDescriptor);
        }
        else
        {
            throw new ArgumentException($"Expected descriptor of type {nameof(IObjectTypeDescriptor)}");
        }
    }

    protected abstract void RegisterMutation(IObjectTypeDescriptor descriptor);
}
