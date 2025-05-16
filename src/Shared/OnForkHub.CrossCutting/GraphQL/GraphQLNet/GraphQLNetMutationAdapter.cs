using GraphQL;
using GraphQL.Types;

using OnForkHub.Core.Requests;

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetMutationAdapter<TRequest, TResponse>(IGraphQLMutationHandler<TRequest, TResponse> handler, string name, string description)
    : GraphQLNetMutationBase
{
    private readonly IGraphQLMutationHandler<TRequest, TResponse> _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));

    protected override void RegisterMutation(ObjectGraphType descriptor)
    {
        descriptor
            .Field<ObjectGraphType<RequestResult<TResponse>>>(Name)
            .Description(Description)
            .Argument<NonNullGraphType<InputObjectGraphType<TRequest>>>("input", "The input for the mutation")
            .ResolveAsync(async context =>
            {
                var input = context.GetArgument<TRequest>("input");
                var result = await _handler.HandleAsync(input);
                return result;
            });
    }
}
