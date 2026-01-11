using OnForkHub.Core.Interfaces.Configuration;
using OnForkHub.Core.Interfaces.Repositories;
using OnForkHub.Core.Interfaces.Repositories.Base;
using OnForkHub.CrossCutting.DependencyInjection;
using OnForkHub.Persistence.Repositories;
using System.Reflection;

namespace OnForkHub.Api.Extensions
{
    [ExcludeFromCodeCoverage]
    public static class CommonServicesExtension
    {
        public static IServiceCollection AddCustomServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddEndpoints();
            services.AddRavenDbServices(configuration);
            services.AddValidationServices();
            services.AddSpecificServices();
            services.AddValidators();
            services.AddApplicationServices();
            services.AddEntityValidator();
            services.AddEntityFrameworkServices(configuration);
            services.AddRepositories();
            services.AddUseCases();
            services.AddValidationRules();
            return services;
        }

        public static IServiceCollection AddEndpoints(this IServiceCollection services)
        {
            var assembly = typeof(IEndpointAsync).Assembly;
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
                configuration.GetSection("RavenDbSettings").Get<RavenDbSettings>()
                ?? throw new InvalidOperationException("Raven DB settings were not found in the 'RavenDbSettings' section.");

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

        public static IServiceCollection AddSpecificServices(this IServiceCollection services)
        {
            services.AddScoped<ICategoryRepositoryRavenDB, CategoryRepositoryRavenDB>();
            services.AddScoped<ICategoryServiceRavenDB, CategoryServiceRavenDB>();
            services.AddScoped<ICategoryRepositoryEF, CategoryRepositoryEF>();
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
                    var interfaces = type.GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);

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
            var assembly = typeof(IValidationBuilder<>).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var selector = scanner.FindTypesImplementing(typeof(IValidationBuilder<>));
            var strategy = selector.CreateRegistrationStrategy(new LifetimeConfigurator(ServiceLifetime.Scoped));

            strategy.Register(services);

            return services;
        }

        public static void RegisterImplementationsOf(
            this IServiceCollection services,
            Type markerType,
            Type interfaceType,
            ServiceLifetime lifetime = ServiceLifetime.Transient
        )
        {
            var scanner = new AssemblyScanner(markerType.Assembly);
            var typeSelector = scanner.FindTypesImplementing(interfaceType);
            var configurator = new LifetimeConfigurator(lifetime);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);
        }

        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            var assembly = typeof(IValidationRule<>).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var typeSelector = scanner.FindTypesImplementing(typeof(IValidationRule<>));
            var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            var assembly = typeof(IBaseRepository<>).Assembly;
            var scanner = new AssemblyScanner(assembly);
            var typeSelector = scanner.FindTypesImplementing(typeof(IBaseRepository<>));
            var configurator = new LifetimeConfigurator(ServiceLifetime.Scoped);
            var strategy = typeSelector.CreateRegistrationStrategy(configurator);

            strategy.Register(services);

            return services;
        }

        private static IServiceCollection AddUseCases(this IServiceCollection services)
        {
            var assemblies = new[] { typeof(IUseCase<,>).Assembly, typeof(GetAllCategoryUseCase).Assembly, typeof(CreateCategoryUseCase).Assembly };

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
