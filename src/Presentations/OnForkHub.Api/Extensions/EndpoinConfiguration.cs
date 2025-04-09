namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class EndpoinConfiguration
{
    public static void AddEndpoin(this IServiceCollection services, Type markerType)
    {
        services.RegisterImplementationsOf<IEndpoint>(markerType);
        services.RegisterImplementationsOf<IEndpointAsync>(markerType);
    }

    public static async Task UseEndpoinAsync(this WebApplication app)
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
