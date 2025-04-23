using System.Reflection;

namespace OnForkHub.CrossCutting.GraphQL.HotChocolate;

public class HotChocolateConfigurator : IGraphQLConfigurator
{
    public void RegisterGraphQLServices(IServiceCollection services)
    {
        var requestExecutorBuilder = services.AddGraphQLServer();
        var hotChocolateSchemaBuilder = new HotChocolateSchemaBuilder(requestExecutorBuilder);

        RegisterOperations(hotChocolateSchemaBuilder);

        hotChocolateSchemaBuilder.Build();

        services.AddSingleton<IGraphQLSchemaBuilder>(hotChocolateSchemaBuilder);
    }

    private static void RegisterOperations(HotChocolateSchemaBuilder graphQLSchemaBuilder)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IGraphQLQuery).IsAssignableFrom(t)))
                {
                    var query = (IGraphQLQuery)Activator.CreateInstance(type)!;
                    graphQLSchemaBuilder.AddQuery(query);
                }

                foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IGraphQLMutation).IsAssignableFrom(t)))
                {
                    var mutation = (IGraphQLMutation)Activator.CreateInstance(type)!;
                    graphQLSchemaBuilder.AddMutation(mutation);
                }
            }
            catch (ReflectionTypeLoadException)
            {
                continue;
            }
        }
    }
}
