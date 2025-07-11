using Microsoft.EntityFrameworkCore;

using OnForkHub.Api.Endpoints.Rest.V1.Categories;
using OnForkHub.Application.DependencyInjection;
using OnForkHub.Application.Services.Base;
using OnForkHub.Persistence.Configurations;
using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;

using Raven.Client.Documents;

namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class CommonServicesExtension
{
    public static IServiceCollection AddCustomServices(this IServiceCollection services)
    {
        try
        {
            services.Scan(scan =>
                scan.FromAssemblyOf<CreateEndpoint>().AddClassesEndingWith("Endpoint").AsImplementedInterfaces().WithScopedLifetime()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar endpoints: {ex.Message}");
        }

        try
        {
            services.Scan(scan =>
                scan.FromAssemblyOf<CategoryValidator>()
                    .AddClassesImplementing(typeof(IEntityValidator<>))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar validators: {ex.Message}");
        }

        try
        {
            services.Scan(scan =>
                scan.FromAssemblyOf<GetAllCategoryUseCase>()
                    .AddClassesImplementing(typeof(IUseCase<,>))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar use cases: {ex.Message}");
        }

        try
        {
            services.Scan(scan =>
                scan.FromAssemblyOf<CategoryNameValidationRule>()
                    .AddClassesImplementing(typeof(IValidationRule<>))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar validation rules: {ex.Message}");
        }

        try
        {
            services.Scan(scan => scan.FromAssemblyOf<BaseService>().AddClassesEndingWith("Service").AsImplementedInterfaces().WithScopedLifetime());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar services: {ex.Message}");
        }

        try
        {
            services.Scan(scan =>
                scan.FromAssemblyOf<EntityFrameworkDataContext>()
                    .AddClassesEndingWith("Repository", "RepositoryEF", "RepositoryRavenDB")
                    .AsImplementedInterfaces()
                    .WithScopedLifetime()
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao registrar repositórios: {ex.Message}");
        }

        services.AddScoped(typeof(IValidationBuilder<>), typeof(ValidationBuilder<>));
        services.AddScoped(typeof(IValidationService<>), typeof(ValidationService<>));
        services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();

        return services;
    }

    public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<EntityFrameworkDataContext>(options => options.UseSqlServer(connectionString));
        services.AddScoped<IEntityFrameworkDataContext, EntityFrameworkDataContext>();
        return services;
    }

    public static IServiceCollection AddRavenDbServices(this IServiceCollection services, IConfiguration configuration)
    {
        var ravenDbSettings = configuration.GetSection("RavenDbSettings").Get<RavenDbSettings>();

        services.AddSingleton<IDocumentStore>(serviceProvider =>
        {
            var store = new DocumentStore { Urls = ravenDbSettings?.Urls, Database = ravenDbSettings?.Database };
            store.Initialize();
            return store;
        });

        services.AddSingleton<RavenDbDataContext>();
        return services;
    }
}
