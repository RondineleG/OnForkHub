using OnForkHub.CrossCutting.DependencyInjection;

namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class EndpoinExtension
{
    public static void AddEndpoin(this IServiceCollection services, Type markerType)
    {
        var scanner = new AssemblyScanner(markerType.Assembly);

        var endpointTypeSelector = scanner.FindTypesImplementing<IEndpoint>();
        var endpointAsyncTypeSelector = scanner.FindTypesImplementing<IEndpointAsync>();

        var configurator = new LifetimeConfigurator(ServiceLifetime.Transient);

        var endpointStrategy = endpointTypeSelector.CreateRegistrationStrategy(configurator);
        var endpointAsyncStrategy = endpointAsyncTypeSelector.CreateRegistrationStrategy(configurator);

        endpointStrategy.Register(services);
        endpointAsyncStrategy.Register(services);
    }

    public static async Task UseEndpointsAsync(this WebApplication app)
    {
        using (var scope = app.Services.CreateScope())
        {
            var scopedProvider = scope.ServiceProvider;
            var webApis = scopedProvider.GetServices<IEndpoint>();
            foreach (var webApi in webApis)
            {
                webApi.Register(app);
            }

            var asyncWebApis = scopedProvider.GetServices<IEndpointAsync>();
            await Task.WhenAll(asyncWebApis.Select(x => x.RegisterAsync(app)));
        }
    }
}
