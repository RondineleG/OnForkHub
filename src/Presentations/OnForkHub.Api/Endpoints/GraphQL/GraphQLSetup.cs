namespace OnForkHub.Api.Endpoints.GraphQL;

public static class GraphQLSetup
{
    public static IRequestExecutorBuilder AddMutations(this IRequestExecutorBuilder builder)
    {
        return builder.AddMutationType(d =>
        {
            var mutationTypes = typeof(Program)
                .Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsClass && typeof(MutationGraphQLBase).IsAssignableFrom(t));

            foreach (var mutationType in mutationTypes)
            {
                var instance = Activator.CreateInstance(mutationType) as MutationGraphQLBase;
                instance?.Register(d);
            }
        });
    }

    public static IRequestExecutorBuilder AddQueries(this IRequestExecutorBuilder builder)
    {
        return builder.AddQueryType(d =>
        {
            var queryTypes = typeof(Program)
                .Assembly.GetTypes()
                .Where(t => !t.IsAbstract && t.IsClass && typeof(QueryGraphQLBase).IsAssignableFrom(t));

            foreach (var queryType in queryTypes)
            {
                var instance = Activator.CreateInstance(queryType) as QueryGraphQLBase;
                instance?.Register(d);
            }
        });
    }
}
