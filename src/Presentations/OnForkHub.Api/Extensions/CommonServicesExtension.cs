using System.Reflection;

using OnForkHub.Application.Services;
using OnForkHub.Application.UseCases.Categories;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Repositories.Base;
using OnForkHub.Core.Interfaces.Services;
using OnForkHub.Core.Interfaces.Validations;
using OnForkHub.Core.Responses;
using OnForkHub.Core.Validations;
using OnForkHub.CrossCutting.DependencyInjection;
using OnForkHub.CrossCutting.Interfaces;
using OnForkHub.CrossCutting.Storage;
using OnForkHub.Persistence.Contexts;
using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Repositories;

using Raven.Client.Documents;

namespace OnForkHub.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class CommonServicesExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpoints();
            services.AddRavenDbServices(configuration);
            services.AddSpecificServices(configuration);
            services.AddValidators();
            services.AddEntityFrameworkServices(configuration);
            services.AddUseCases();
            services.AddValidationServices();
            services.AddValidationRules();
            return services;
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            // Only scan the API assembly for endpoints to avoid duplicates
            var assembly = typeof(CommonServicesExtension).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var typeSelector = scanner.FindTypesImplementing<IEndpointAsync>();
            var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);

            return services;
        }

        public static IServiceCollection AddEntityFrameworkServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection String 'DefaultConnection' has not been found or is empty.");

            services.AddDbContext<EntityFrameworkDataContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IEntityFrameworkDataContext, EntityFrameworkDataContext>();

            return services;
        }

        public static IServiceCollection AddEntityValidator(this IServiceCollection services)
        {
            var assembly = typeof(IEntityValidator<>).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var typeSelector = scanner.FindTypesImplementing(typeof(IEntityValidator<>));
            var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);

            return services;
        }

        public static IServiceCollection AddRavenDbServices(this IServiceCollection services, IConfiguration configuration)
        {
            var ravenDbSettings =
                configuration.GetSection("RavenDbSettings").Get<RavenDbSettings>() ?? throw new InvalidOperationException(
                    "Raven DB settings were not found in the 'RavenDbSettings' section."
                );

            if (ravenDbSettings.Urls is null || ravenDbSettings.Urls.Length == 0)
                throw new InvalidOperationException("Raven DB URLs were not configured.");

            if (string.IsNullOrWhiteSpace(ravenDbSettings.Database))
                throw new InvalidOperationException("Raven DB Database Name has not been configured.");

            services.AddSingleton<IDocumentStore>(_ =>
            {
                var store = new DocumentStore { Urls = ravenDbSettings.Urls, Database = ravenDbSettings.Database };
                store.Initialize();
                return store;
            });

            services.AddSingleton<RavenDbDataContext>();

            return services;
        }

        public static IServiceCollection AddSpecificServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ICategoryRepositoryRavenDB, CategoryRepositoryRavenDB>();
            services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();
            services.AddScoped<ICategoryRepositoryEF, CategoryRepositoryEF>();
            services.AddScoped<IVideoRepositoryEF, VideoRepositoryEF>();
            services.AddScoped<IValidationService<Video>, ValidationService<Video>>();
            services.AddScoped<IVideoService, VideoService>();
            services.AddScoped<IVideoUploadService, VideoUploadService>();
            services.AddScoped<IVideoTranscodingService, VideoTranscodingService>();
            services.AddScoped<IRecommendationService, RecommendationService>();
            services.AddSingleton<DashManifestGenerator>();
            services.AddScoped<INotificationRepositoryEF, NotificationRepositoryEF>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IUserRepositoryEF, UserRepositoryEF>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRefreshTokenRepositoryEF, RefreshTokenRepositoryEF>();
            services.AddScoped<IVideoUploadRepository, VideoUploadRepositoryEF>();

            // File Storage Configuration
            services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
            services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));

            var storageProvider = configuration.GetValue<string>("FileStorage:Provider") ?? "Local";
            if (storageProvider.Equals("Azure", StringComparison.OrdinalIgnoreCase))
            {
                services.AddScoped<IFileStorageService, AzureBlobStorageService>();
            }
            else
            {
                services.AddScoped<IFileStorageService, LocalFileStorageService>();
            }

            services.AddHostedService<VideoProcessingBackgroundService>();

            return services;
        }

        public static IServiceCollection AddUseCasesAuto(this IServiceCollection services, params Assembly[] assemblies)
        {
            if (assemblies == null || assemblies.Length == 0)
                throw new ArgumentException("You must provide at least one assembly to scan.");

            var interfaceType = typeof(IUseCase<,>);
            var lifetime = ServiceLifetime.Scoped;

            foreach (var assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface);

                foreach (var type in types)
                {
                    var interfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).ToArray();
                    if (interfaces.Length == 0)
                    {
                        continue;
                    }

                    services.Add(new ServiceDescriptor(type, type, lifetime));

                    foreach (var serviceInterface in interfaces)
                    {
                        services.Add(new ServiceDescriptor(serviceInterface, type, lifetime));
                    }
                }
            }

            return services;
        }

        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            services.AddScoped(typeof(IValidationBuilder<>), typeof(ValidationBuilder<>));

            // Scan for IEntityValidator<> implementations in Core
            var coreAssembly = typeof(IEntityValidator<>).Assembly;
            var scanner = new AssemblyScanner(coreAssembly);
            var selector = scanner.FindTypesImplementing(typeof(IEntityValidator<>));
            var strategy = selector.CreateRegistrationStrategy(new LifetimeConfigurator(ServiceLifetime.Scoped));
            strategy.Register(services);

            return services;
        }

        private static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            var assemblies = new[] { typeof(IUseCase<,>).Assembly, typeof(GetAllCategoryUseCase).Assembly };

            return services.AddUseCasesAuto(assemblies);
        }

        private static IServiceCollection AddValidationRules(this IServiceCollection services)
        {
            var assembly = typeof(IValidationRule<>).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var typeSelector = scanner.FindTypesImplementing(typeof(IValidationRule<>));
            var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);

            return services;
        }

        private static IServiceCollection AddValidationServices(this IServiceCollection services)
        {
            var assembly = typeof(IValidationService<>).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var typeSelector = scanner.FindTypesImplementing(typeof(IValidationService<>));
            var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);

            return services;
        }
    }
}
