// The .NET Foundation licenses this file to you under the MIT license.

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
            foreach (
                var type in assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && typeof(IGraphQLQuery).IsAssignableFrom(t))
            )
            {
                if (Activator.CreateInstance(type) is IGraphQLQuery instance)
                {
                    graphQLSchemaBuilder.AddQuery(instance);
                }
            }

            foreach (
                var type in assembly
                    .GetTypes()
                    .Where(t => !t.IsAbstract && !t.IsGenericTypeDefinition && typeof(IGraphQLMutation).IsAssignableFrom(t))
            )
            {
                if (Activator.CreateInstance(type) is IGraphQLMutation instance)
                {
                    graphQLSchemaBuilder.AddMutation(instance);
                }
            }
        }
    }
}
