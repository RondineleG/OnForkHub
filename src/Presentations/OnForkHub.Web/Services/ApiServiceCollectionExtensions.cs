namespace OnForkHub.Web.Services;

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Web.Services.Api;

/// <summary>
/// Extension methods for registering Web API services.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds all Web API services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    public static void AddApiServices(this IServiceCollection services)
    {
        // API Services
        services.AddScoped<IVideoService, VideoService>();
        services.AddScoped<IVideoUploadService, VideoUploadService>();
        services.AddScoped<IRecommendationService, RecommendationService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IUserService, UserService>();
    }
}
