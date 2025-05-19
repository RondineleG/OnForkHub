// The .NET Foundation licenses this file to you under the MIT license.

using HotChocolate.Types;

using OnForkHub.Core.Requests;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateQueryAdapter<TRequest, TResponse>(IGraphQLQueryHandler<TRequest, TResponse> handler, string name, string description)
    : HotChocolateQueryBase
{
    private readonly IGraphQLQueryHandler<TRequest, TResponse> _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    protected override void RegisterQuery(IObjectTypeDescriptor descriptor)
    {
        descriptor
            .Field(Name)
            .Argument("input", a => a.Type<NonNullType<InputObjectType<TRequest>>>())
            .Resolve(async context =>
            {
                var input = context.ArgumentValue<TRequest>("input");
                return await _handler.HandleAsync(input);
            })
            .Type<ObjectType<RequestResult<TResponse>>>()
            .Description(Description);
    }
}
