using HotChocolate.Types;

using OnForkHub.Core.Requests;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateMutationAdapter<TRequest, TResponse>(IGraphQLMutationHandler<TRequest, TResponse> handler, string name, string description)
    : HotChocolateMutationBase
{
    private readonly IGraphQLMutationHandler<TRequest, TResponse> _handler = handler ?? throw new ArgumentNullException(nameof(handler));
    public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public override string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    protected override void RegisterMutation(IObjectTypeDescriptor descriptor)
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
