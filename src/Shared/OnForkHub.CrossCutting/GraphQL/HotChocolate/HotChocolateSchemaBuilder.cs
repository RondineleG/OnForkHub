using HotChocolate.Execution.Configuration;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateSchemaBuilder(IRequestExecutorBuilder executorBuilder) : IGraphQLSchemaBuilder
{
    private readonly IRequestExecutorBuilder _executorBuilder = executorBuilder;
    private readonly List<IGraphQLQuery> _queries = [];
    private readonly List<IGraphQLMutation> _mutations = [];

    public IGraphQLSchemaBuilder AddQuery(IGraphQLQuery query)
    {
        _queries.Add(query);
        return this;
    }

    public IGraphQLSchemaBuilder AddMutation(IGraphQLMutation mutation)
    {
        _mutations.Add(mutation);
        return this;
    }

    public object Build()
    {
        _executorBuilder.AddQueryType(descriptor =>
        {
            descriptor.Name("Query");

            foreach (var query in _queries)
            {
                query.Register(descriptor);
            }
        });

        _executorBuilder.AddMutationType(descriptor =>
        {
            descriptor.Name("Mutation");

            foreach (var mutation in _mutations)
            {
                mutation.Register(descriptor);
            }
        });

        return _executorBuilder;
    }
}