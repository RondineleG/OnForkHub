using GraphQL.Types;

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public abstract class GraphQLNetQueryBase : IGraphQLQuery
{
    public abstract string Name { get; }
    public abstract string Description { get; }

    public void Register(object descriptor)
    {
        if (descriptor is ObjectGraphType graphType)
        {
            RegisterQuery(graphType);
        }
        else
        {
            throw new ArgumentException($"Expected descriptor of type {nameof(ObjectGraphType)}");
        }
    }

    protected abstract void RegisterQuery(ObjectGraphType graphType);
}