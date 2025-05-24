using GraphQL;

using OnForkHub.Core.Interfaces.GraphQL;

namespace OnForkHub.CrossCutting.GraphQL.GraphQLNet;

public class GraphQLNetConfigurator : IGraphQLConfigurator
{
    public void RegisterGraphQLServices(IServiceCollection services)
    {
        services.AddScoped(sp =>
        {
            var schemaBuilder = new GraphQLNetSchemaBuilder(sp);
            RegisterOperations(schemaBuilder);
            return (Schema)schemaBuilder.Build();
        });

        services.AddGraphQL();
        services.Configure<ExecutionOptions>(options =>
        {
            options.EnableMetrics = true;
        });
    }

    private static void RegisterOperations(GraphQLNetSchemaBuilder schemaBuilder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IGraphQLQuery).IsAssignableFrom(t)))
            {
                if (Activator.CreateInstance(type) is IGraphQLQuery query)
                {
                    schemaBuilder.AddQuery(query);
                }
            }

            foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IGraphQLMutation).IsAssignableFrom(t)))
            {
                if (Activator.CreateInstance(type) is IGraphQLMutation mutation)
                {
                    schemaBuilder.AddMutation(mutation);
                }
            }
        }
    }
}
