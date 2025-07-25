// The .NET Foundation licenses this file to you under the MIT license.

using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.Core.GraphQL;

public abstract class MutationGraphQLBase : IEndpointGraphQL
{
    public abstract void Register(IObjectTypeDescriptor descriptor);
}
