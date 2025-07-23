using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Core.Interfaces.DependencyInjection;

using System.Reflection;
using System.Runtime.CompilerServices;

namespace OnForkHub.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services.Scan(scan => scan.FromLoadedAssemblies().AddClassesWithAutoRegisterAttribute());
    }

    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (assemblies == null || assemblies.Length == 0)
        {
            throw new ArgumentException("At least one assembly must be provided", nameof(assemblies));
        }

        return services.Scan(scan => scan.FromAssemblies(assemblies).AddClassesWithAutoRegisterAttribute());
    }

    public static IRegistrationStrategy AddClassesEndingWith(this ITypeSelector selector, params string[] suffixes)
    {
        ArgumentNullException.ThrowIfNull(selector);

        if (suffixes == null || suffixes.Length == 0)
            throw new ArgumentException("At least one suffix should be provided", nameof(suffixes));

        return selector.AddClasses(type =>
            IsConcreteClass(type)
            && suffixes.Any(suffix => !string.IsNullOrEmpty(suffix) && type.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
        );
    }

    public static IServiceCollection Scan(this IServiceCollection services, Action<IAssemblyScanner> configureScanner)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureScanner);

        var scanner = new AssemblyScanner(services, null);
        configureScanner(scanner);

        return services;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsConcreteClass(Type type) =>
        type != null && type.IsClass && !type.IsAbstract && !type.IsInterface && !type.IsNested && type.IsPublic;
}
