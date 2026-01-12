using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Attributes;

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class ServiceRegister
{
    public ServiceRegister(IServiceCollection services, ILogger? logger = null)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger;
    }

    private static readonly Type[] EmptyTypes = Array.Empty<Type>();

    private static readonly FrozenSet<Type> IgnoredInterfaces = new[] { typeof(IDisposable), typeof(IAsyncDisposable) }.ToFrozenSet();

    private static readonly ConcurrentDictionary<Type, Type[]> InterfacesCache = new();

    private static readonly Action<ILogger, string, string, Exception?> LogDebugTypeRegistered = LoggerMessage.Define<string, string>(
        LogLevel.Debug,
        new EventId(6, nameof(LogDebugTypeRegistered)),
        "Registered {Type} as {ServiceType}"
    );

    private static readonly Action<ILogger, string, Exception?> LogDebugTypeRegisteredSelf = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(7, nameof(LogDebugTypeRegisteredSelf)),
        "Registered {Type} as self"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogErrorRegisteringType = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(3, nameof(LogErrorRegisteringType)),
        "Error when registering type '{TypeName}': {InnerErrorMessage}"
    );

    private static readonly Action<ILogger, string, Exception?> LogNoTypesDiscovered = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(8, nameof(LogNoTypesDiscovered)),
        "{Message}"
    );

    private static readonly FrozenSet<string> SystemNamespaces = new[] { "System", "Microsoft", "mscorlib" }.ToFrozenSet(StringComparer.Ordinal);

    private readonly ILogger? _logger;

    private readonly IServiceCollection _services;

    private readonly Stopwatch _stopwatch = new();

    public RegistrationResult RegisterAll(
        IReadOnlyList<Type> typesToRegister,
        ERegistrationStrategyType strategy,
        ServiceLifetime lifetime,
        ERegistrationMode mode,
        Type[]? serviceTypes = null,
        Func<IServiceProvider, object>? factory = null
    )
    {
        if (typesToRegister.Count == 0)
        {
            LogNoTypesDiscoveredIfEnabled("No types have been discovered for registration. Check the filters applied.");
            return new RegistrationResult(EmptyTypes, TimeSpan.Zero);
        }

        _stopwatch.Restart();
        var registeredTypes = new List<Type>(typesToRegister.Count);

        var typesSpan = CollectionsMarshal.AsSpan(typesToRegister.ToList());
        for (int i = 0; i < typesSpan.Length; i++)
        {
            var implementationType = typesSpan[i];
            try
            {
                if (RegisterType(implementationType, strategy, lifetime, mode, serviceTypes, factory))
                    registeredTypes.Add(implementationType);
            }
            catch (Exception ex)
            {
                LogErrorRegisteringTypeIfEnabled(ex.Message, implementationType.Name, ex);
                throw;
            }
        }

        _stopwatch.Stop();
        return new RegistrationResult(registeredTypes.AsReadOnly(), _stopwatch.Elapsed);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Type[] GetCachedImplementedInterfaces(Type type) =>
        InterfacesCache.GetOrAdd(
            type,
            t =>
            {
                var interfaces = t.GetInterfaces();
                return interfaces.Where(iface => !IgnoredInterfaces.Contains(iface) && !IsSystemType(iface)).ToArray();
            }
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ImplementsOpenGenericInterface(Type type, Type openGenericInterface)
    {
        var interfaces = type.GetInterfaces();
        for (int i = 0; i < interfaces.Length; i++)
        {
            var iface = interfaces[i];
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == openGenericInterface)
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAssignableFromCached(Type serviceType, Type implementationType)
    {
        if (serviceType.IsGenericTypeDefinition)
            return ImplementsOpenGenericInterface(implementationType, serviceType);
        return serviceType.IsAssignableFrom(implementationType);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSystemType(Type type)
    {
        var ns = type.Namespace;
        return !string.IsNullOrEmpty(ns) && SystemNamespaces.Any(sysNs => ns.StartsWith(sysNs, StringComparison.Ordinal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddDescriptor(ServiceDescriptor descriptor, ERegistrationMode mode)
    {
        switch (mode)
        {
            case ERegistrationMode.Default:
                _services.Add(descriptor);
                break;

            case ERegistrationMode.TryAdd:
                _services.TryAdd(descriptor);
                break;

            case ERegistrationMode.TryAddEnumerable:
                _services.TryAddEnumerable(descriptor);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Invalid registration mode");
        }
    }

    private void LogDebugTypeRegisteredIfEnabled(string typeName, string serviceTypeName)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            LogDebugTypeRegistered(_logger, typeName, serviceTypeName, null);
    }

    private void LogDebugTypeRegisteredSelfIfEnabled(string typeName)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            LogDebugTypeRegisteredSelf(_logger, typeName, null);
    }

    private void LogErrorRegisteringTypeIfEnabled(string message, string typeName, Exception ex)
    {
        if (_logger?.IsEnabled(LogLevel.Error) == true)
            LogErrorRegisteringType(_logger, message, typeName, ex);
    }

    private void LogNoTypesDiscoveredIfEnabled(string message)
    {
        if (_logger?.IsEnabled(LogLevel.Information) == true)
            LogNoTypesDiscovered(_logger, message, null);
    }

    private bool RegisterAsImplementedInterfaces(Type implementationType, ServiceLifetime lifetime, ERegistrationMode mode)
    {
        var interfaces = GetCachedImplementedInterfaces(implementationType);

        if (interfaces.Length == 0)
            return RegisterAsSelf(implementationType, lifetime, mode);

        foreach (var serviceType in interfaces)
        {
            AddDescriptor(new ServiceDescriptor(serviceType, implementationType, lifetime), mode);
            LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
        }

        return true;
    }

    private bool RegisterAsSelf(Type implementationType, ServiceLifetime lifetime, ERegistrationMode mode)
    {
        AddDescriptor(new ServiceDescriptor(implementationType, implementationType, lifetime), mode);
        LogDebugTypeRegisteredSelfIfEnabled(implementationType.Name);
        return true;
    }

    private bool RegisterAsSpecificTypes(Type implementationType, ServiceLifetime lifetime, ERegistrationMode mode, Type[]? serviceTypes)
    {
        if (serviceTypes is null || serviceTypes.Length == 0)
            throw new InvalidOperationException("Service types were not specified.");

        var registered = false;
        foreach (var serviceType in serviceTypes)
        {
            if (IsAssignableFromCached(serviceType, implementationType))
            {
                AddDescriptor(new ServiceDescriptor(serviceType, implementationType, lifetime), mode);
                LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
                registered = true;
            }
        }

        return registered;
    }

    private bool RegisterType(
        Type implementationType,
        ERegistrationStrategyType strategy,
        ServiceLifetime lifetime,
        ERegistrationMode mode,
        Type[]? serviceTypes,
        Func<IServiceProvider, object>? factory
    )
    {
        var autoRegisterAttr = implementationType.GetCustomAttribute<AutoRegisterAttribute>();
        if (autoRegisterAttr is not null)
            return RegisterTypeWithAttribute(implementationType, autoRegisterAttr, mode);

        return strategy switch
        {
            ERegistrationStrategyType.AsImplementedInterfaces => RegisterAsImplementedInterfaces(implementationType, lifetime, mode),
            ERegistrationStrategyType.AsSelf => RegisterAsSelf(implementationType, lifetime, mode),
            ERegistrationStrategyType.AsSpecificTypes => RegisterAsSpecificTypes(implementationType, lifetime, mode, serviceTypes),
            ERegistrationStrategyType.UsingFactory => RegisterUsingFactory(lifetime, mode, serviceTypes, factory),
            _ => false,
        };
    }

    private bool RegisterTypeWithAttribute(Type implementationType, AutoRegisterAttribute attribute, ERegistrationMode mode)
    {
        try
        {
            var registered = false;

            if (attribute.AsTypes?.Length > 0)
            {
                foreach (var serviceType in attribute.AsTypes)
                {
                    if (!IsAssignableFromCached(serviceType, implementationType))
                        continue;

                    AddDescriptor(new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime), mode);
                    LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
                    registered = true;
                }
                return registered;
            }

            if (attribute.AsImplementedInterfaces)
            {
                var interfaces = GetCachedImplementedInterfaces(implementationType);
                foreach (var serviceType in interfaces)
                {
                    AddDescriptor(new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime), mode);
                    LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
                    registered = true;
                }

                if (interfaces.Length == 0 && attribute.AsSelf)
                {
                    AddDescriptor(new ServiceDescriptor(implementationType, implementationType, attribute.Lifetime), mode);
                    LogDebugTypeRegisteredSelfIfEnabled(implementationType.Name);
                    registered = true;
                }
            }

            if (attribute.AsSelf)
            {
                AddDescriptor(new ServiceDescriptor(implementationType, implementationType, attribute.Lifetime), mode);
                LogDebugTypeRegisteredSelfIfEnabled(implementationType.Name);
                registered = true;
            }

            return registered;
        }
        catch (Exception ex)
        {
            LogErrorRegisteringTypeIfEnabled(ex.Message, implementationType.Name, ex);
            throw;
        }
    }

    private bool RegisterUsingFactory(ServiceLifetime lifetime, ERegistrationMode mode, Type[]? serviceTypes, Func<IServiceProvider, object>? factory)
    {
        if (factory is null || serviceTypes is null || serviceTypes.Length == 0)
            throw new InvalidOperationException("Factory or service types have not been specified.");

        foreach (var serviceType in serviceTypes)
        {
            AddDescriptor(new ServiceDescriptor(serviceType, factory, lifetime), mode);
        }

        return true;
    }
}
