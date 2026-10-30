using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Application.Services;
using OnForkHub.Core.Validations.Categories;

namespace OnForkHub.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();
        services.AddScoped<IValidationService<Category>, CategoryValidationService>();

        return services;
    }
}
