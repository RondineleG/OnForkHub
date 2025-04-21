using OnForkHub.CrossCutting.GraphQL.GraphQLNet;
using OnForkHub.CrossCutting.GraphQL.HotChocolate;

namespace OnForkHub.CrossCutting.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphQLFromCrossCutting(this IServiceCollection services)
    {
        services.AddSingleton<IGraphQLConfigurator, HotChocolateConfigurator>();
        var configurator = new HotChocolateConfigurator();
        configurator.RegisterGraphQLServices(services);

        // Criar o gerenciador de endpoints
        var endpointManager = new GraphQLEndpointManager();

        // Registrar os endpoints
        endpointManager.RegisterEndpoint(new HotChocolateEndpoint());
        endpointManager.RegisterEndpoint(new GraphQLNetEndpoint());

        // Configurar todos os endpoints
        endpointManager.ConfigureAll(services);

        // Registrar o gerenciador para DI
        return services;
    }
}
