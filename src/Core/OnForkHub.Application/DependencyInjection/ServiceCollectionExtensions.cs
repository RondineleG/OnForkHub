using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Interfaces.DependencyInjection;

using System.Reflection;

namespace OnForkHub.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services)
    {
        return services.Scan(scan => scan.FromLoadedAssemblies().AddClassesWithAutoRegisterAttribute());
    }

    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        return services.Scan(scan => scan.FromAssemblies(assemblies).AddClassesWithAutoRegisterAttribute());
    }

    public static IRegistrationStrategy AddClassesContaining(this ITypeSelector selector, params string[] patterns)
    {
        if (patterns == null || patterns.Length == 0)
            throw new ArgumentException("Pelo menos um padrão deve ser fornecido", nameof(patterns));

        return selector.AddClasses(type =>
            IsConcreteClass(type) && patterns.Any(pattern => type.Name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
        );
    }

    public static IRegistrationStrategy AddClassesEndingWith(this ITypeSelector selector, params string[] suffixes)
    {
        if (suffixes == null || suffixes.Length == 0)
            throw new ArgumentException("Pelo menos um sufixo deve ser fornecido", nameof(suffixes));

        return selector.AddClasses(type =>
            IsConcreteClass(type) && suffixes.Any(suffix => type.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        );
    }

    public static IRegistrationStrategy AddClassesStartingWith(this ITypeSelector selector, params string[] prefixes)
    {
        if (prefixes == null || prefixes.Length == 0)
            throw new ArgumentException("Pelo menos um prefixo deve ser fornecido", nameof(prefixes));

        return selector.AddClasses(type =>
            IsConcreteClass(type) && prefixes.Any(prefix => type.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        );
    }

    public static void ClearAssemblyCache()
    {
        AssemblyCache.ClearCache();
    }

    public static IServiceCollection Scan(this IServiceCollection services, Action<IAssemblyScanner> configureScanner)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureScanner);

        var serviceProvider = services.BuildServiceProvider();
        var logger = serviceProvider.GetService<ILogger<AssemblyScanner>>();

        var scanner = new AssemblyScanner(services, logger);
        configureScanner(scanner);

        return services;
    }

    private static bool IsConcreteClass(Type type) => type.IsClass && !type.IsAbstract && !type.IsInterface && !type.IsNested && type.IsPublic;
}
