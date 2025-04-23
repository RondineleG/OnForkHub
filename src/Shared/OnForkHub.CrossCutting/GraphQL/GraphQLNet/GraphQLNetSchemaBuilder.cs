using GraphQL.Types;

using Schema = GraphQL.Types.Schema;

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetSchemaBuilder : IGraphQLSchemaBuilder, IDisposable
{
    private readonly List<IGraphQLQuery> _queries = [];
    private readonly List<IGraphQLMutation> _mutations = [];

    private readonly Schema _schema;
    private readonly QueryGraphType _queryType;
    private readonly MutationGraphType _mutationType;

    public GraphQLNetSchemaBuilder(IServiceProvider serviceProvider)
    {
        _queryType = new QueryGraphType();
        _mutationType = new MutationGraphType();

        _schema = new Schema(serviceProvider) { Query = _queryType, Mutation = _mutationType };
    }

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
        foreach (var query in _queries)
        {
            query.Register(_queryType);
        }

        foreach (var mutation in _mutations)
        {
            mutation.Register(_mutationType);
        }

        return _schema;
    }

    public void Dispose()
    {
        _schema.Dispose();
        GC.SuppressFinalize(this);
    }

    private sealed class QueryGraphType : ObjectGraphType
    {
        public QueryGraphType()
        {
            Name = "Query";
        }
    }

    private sealed class MutationGraphType : ObjectGraphType
    {
        public MutationGraphType()
        {
            Name = "Mutation";
        }
    }
}
