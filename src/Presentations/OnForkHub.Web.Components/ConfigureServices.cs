namespace OnForkHub.Web.Components;

public static class ConfigureServices
{
    public static IServiceCollection AddComponentServices(this IServiceCollection services)
    {
        services.AddTransient<IVideoPlayerJsInterop, VideoPlayerJsInterop>();
        return services;
    }
}
