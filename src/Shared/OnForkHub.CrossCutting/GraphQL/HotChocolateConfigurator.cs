using OnForkHub.Application.GraphQL.Mutations.Categories;
using OnForkHub.Core.GraphQL;

namespace OnForkHub.CrossCutting.GraphQL;

public class HotChocolateConfigurator : IGraphQLConfigurator
{
    public void ConfigureGraphQL(IServiceCollection services)
    {
        var assembly = typeof(CreateCategoryMutation).Assembly;

        services
            .AddGraphQLServer()
            .AddQueryType(d =>
            {
                d.Name("Query");

                foreach (var type in assembly.GetTypes().Where(t => typeof(QueryGraphQLBase).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    var instance = (IEndpointGraphQL)Activator.CreateInstance(type)!;
                    instance.Register(d);
                }
            })
            .AddMutationType(d =>
            {
                d.Name("Mutation");

                foreach (var type in assembly.GetTypes().Where(t => typeof(MutationGraphQLBase).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    var instance = (IEndpointGraphQL)Activator.CreateInstance(type)!;
                    instance.Register(d);
                }
            });
    }
}