using OnForkHub.Web.Components.Services.Implementations;
using OnForkHub.Web.Components.Services.Interfaces;

namespace OnForkHub.Web.Components;

public static class ConfigureServices
{
    public static IServiceCollection AddComponentServices(this IServiceCollection services)
    {
        services.AddTransient<IVideoPlayerJsInteropService, VideoPlayerJsInteropService>();
        return services;
    }
}
