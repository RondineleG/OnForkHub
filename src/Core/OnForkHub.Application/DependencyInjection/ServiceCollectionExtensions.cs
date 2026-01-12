using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using OnForkHub.Core.Interfaces.DependencyInjection;

namespace OnForkHub.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.Scan(static scan => scan.FromLoadedAssemblies().AddClassesWithAutoRegisterAttribute());
    }

    public static IServiceCollection AddAutoRegisteredServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(assemblies);
        ArgumentOutOfRangeException.ThrowIfZero(assemblies.Length);

        return services.Scan(scan => scan.FromAssemblies(assemblies).AddClassesWithAutoRegisterAttribute());
    }

    public static IRegistrationStrategy AddClassesEndingWith(this ITypeSelector selector, params string[] suffixes)
    {
        ArgumentNullException.ThrowIfNull(selector);
        ArgumentNullException.ThrowIfNull(suffixes);
        ArgumentOutOfRangeException.ThrowIfZero(suffixes.Length);

        return selector.AddClasses(type => IsConcreteClass(type) && HasAnySuffix(type.Name.AsSpan(), suffixes.AsSpan()));
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
    private static bool HasAnySuffix(ReadOnlySpan<char> typeName, ReadOnlySpan<string> suffixes)
    {
        for (var i = 0; i < suffixes.Length; i++)
        {
            var suffix = suffixes[i];
            if (!string.IsNullOrEmpty(suffix) && typeName.EndsWith(suffix.AsSpan(), StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsConcreteClass(Type type) =>
        type.IsClass && !type.IsAbstract && !type.IsInterface && !type.IsNested && type.IsPublic && !type.IsGenericTypeDefinition;
}
