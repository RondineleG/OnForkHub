namespace OnForkHub.CrossCutting.Extensions;

using Microsoft.Extensions.DependencyInjection;

using OnForkHub.CrossCutting.Logging;
using OnForkHub.CrossCutting.Logging.Implementations;

/// <summary>
/// Extension methods for registering logging services.
/// </summary>
public static class LoggingExtensions
{
    /// <summary>
    /// Registers error logger as singleton.
    /// Uses in-memory implementation by default.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorLogging(this IServiceCollection services)
    {
        services.AddSingleton<IErrorLogger, InMemoryErrorLogger>();
        return services;
    }

    /// <summary>
    /// Registers custom error logger implementation.
    /// </summary>
    /// <typeparam name="TImplementation">The error logger implementation type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="lifetime">The service lifetime (default: Singleton).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddErrorLogging<TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Singleton
    )
        where TImplementation : class, IErrorLogger
    {
        services.Add(new ServiceDescriptor(typeof(IErrorLogger), typeof(TImplementation), lifetime));
        return services;
    }
}
