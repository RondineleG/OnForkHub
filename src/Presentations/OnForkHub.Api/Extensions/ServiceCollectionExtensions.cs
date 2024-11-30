using MoreLinq.Extensions;

using System.Diagnostics.CodeAnalysis;

namespace OnForkHub.Api.Extensions;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void RegisterImplementationsOf<T>(
        this IServiceCollection services,
        Type markerType,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    { services.RegisterImplementationsOf(markerType, typeof(T), lifetime); }
    public static void RegisterImplementationsOf(
        this IServiceCollection services,
        Type markerType,
        Type interfaceType,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        markerType.Assembly
            .GetTypes()
            .Where(x => x.DoesImplementInterfaceType(interfaceType))
            .ForEach(
                x => services.Add(
                    new ServiceDescriptor(
                        x.GetInterfaces().First(y => y.IsGenericType ? (y.GetGenericTypeDefinition() == interfaceType) : (y == interfaceType)),
                        x,
                        lifetime)));
    }
    public static bool DoesImplementInterfaceType(this Type type, Type interfaceType)
    {
        return !type.IsAbstract &&
                type.IsClass &&
                type.GetInterfaces().Any(y => y.IsGenericType ? (y.GetGenericTypeDefinition() == interfaceType) : (y == interfaceType));
    }
}