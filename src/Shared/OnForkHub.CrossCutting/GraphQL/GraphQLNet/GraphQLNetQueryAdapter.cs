using GraphQL;
using GraphQL.Types;

using OnForkHub.Core.Requests;

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetQueryAdapter<TRequest, TResponse>(IGraphQLQueryHandler<TRequest, TResponse> handler, string name, string description)
    : GraphQLNetQueryBase
{
    private readonly IGraphQLQueryHandler<TRequest, TResponse> _handler = handler ?? throw new ArgumentNullException(nameof(handler));

    public override string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public override string Description { get; } = description ?? throw new ArgumentNullException(nameof(description));

    protected override void RegisterQuery(ObjectGraphType graphType)
    {
        graphType
            .Field<ListGraphType<ObjectGraphType<RequestResult<TResponse>>>>(Name)
            .Description(Description)
            .ResolveAsync(async context =>
            {
                var input = context.GetArgument<TRequest>("input");
                var result = await _handler.HandleAsync(input);
                return result.Data;
            });
    }
}