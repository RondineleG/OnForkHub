using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Interfaces.DependencyInjection;

using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class AssemblyScanner : IAssemblyScanner, ITypeSelector, IRegistrationStrategy, ILifetimeConfigurator
{
    public AssemblyScanner(IServiceCollection services, ILogger<AssemblyScanner>? logger = null)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _logger = logger;
    }

    private static readonly ConcurrentDictionary<Assembly, Type[]> AssemblyTypesCache = new();

    private static readonly ConcurrentDictionary<(Type, Type), bool> AssignabilityCache = new();

    private static readonly Type[] EmptyTypes = Array.Empty<Type>();

    private static readonly FrozenSet<Type> IgnoredInterfaces = new[] { typeof(IDisposable), typeof(IAsyncDisposable) }.ToFrozenSet();

    private static readonly ConcurrentDictionary<Type, Type[]> InterfacesCache = new();

    private static readonly Func<Assembly, bool> IsNotSystemAssemblyPredicate = assembly => !IsSystemAssembly(assembly);

    private static readonly Action<ILogger, string, Exception?> LogAssemblyAdded = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(2, nameof(LogAssemblyAdded)),
        "Assembly adicionado: {Assembly}"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogAssemblyLoadFailed = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(2, nameof(LogAssemblyLoadFailed)),
        "Falha ao carregar o assembly '{AssemblyName}': {ErrorMessage}"
    );

    private static readonly Action<ILogger, string, Exception?> LogAssemblyNotFound = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(3, nameof(LogAssemblyNotFound)),
        "Assembly não encontrado: {AssemblyName}"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogDebugTypeRegistered = LoggerMessage.Define<string, string>(
        LogLevel.Debug,
        new EventId(6, nameof(LogDebugTypeRegistered)),
        "Registrado {Type} como {ServiceType}"
    );

    private static readonly Action<ILogger, string, Exception?> LogDebugTypeRegisteredSelf = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(7, nameof(LogDebugTypeRegisteredSelf)),
        "Registrado {Type} como self"
    );

    private static readonly Action<ILogger, int, Exception?> LogDiscoveredTypes = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(1, nameof(LogDiscoveredTypes)),
        "Descobertos {Count} tipos para registro"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogErrorRegisteringType = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(3, nameof(LogErrorRegisteringType)),
        "Erro ao registrar o tipo '{TypeName}': {InnerErrorMessage}"
    );

    private static readonly Action<ILogger, string, Exception?> LogNoTypesDiscovered = LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(8, nameof(LogNoTypesDiscovered)),
        "{Message}"
    );

    private static readonly ConcurrentDictionary<string, Regex> RegexCache = new(Environment.ProcessorCount, 100);

    private static readonly FrozenSet<string> SystemNamespaces = new[] { "System", "Microsoft", "mscorlib" }.ToFrozenSet(StringComparer.Ordinal);

    private readonly HashSet<Assembly> _assemblies = new(ReferenceEqualityComparer.Instance);

    private readonly object _assemblyLock = new();

    private readonly ILogger<AssemblyScanner>? _logger;

    private readonly IServiceCollection _services;

    private readonly Stopwatch _stopwatch = new();

    private readonly List<Type> _typesToRegister = new();

    private bool _allowOpenGenerics;

    private Func<IServiceProvider, object>? _factory;

    private ERegistrationMode _registrationMode = ERegistrationMode.Default;

    private Type[]? _serviceTypes;

    private ERegistrationStrategyType _strategy = ERegistrationStrategyType.AsImplementedInterfaces;

    public IRegistrationStrategy AddClasses(Func<Type, bool>? predicate = null)
    {
        EnsureAssemblies();
        predicate ??= IsValidClassCore;

        var discovered = _assemblies
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .SelectMany(GetCachedTypes)
            .Where(predicate)
            .Where(HasNoExcludeAttribute)
            .ToList();

        LogDiscoveredTypesIfEnabled(discovered.Count);
        _typesToRegister.AddRange(discovered);
        return this;
    }

    public IRegistrationStrategy AddClassesImplementing<TInterface>()
    {
        var interfaceType = typeof(TInterface);
        return AddClasses(type => IsValidClassCore(type) && IsAssignableFromCached(interfaceType, type));
    }

    public IRegistrationStrategy AddClassesImplementing(Type openGenericInterface)
    {
        ArgumentNullException.ThrowIfNull(openGenericInterface);
        if (!openGenericInterface.IsInterface)
            throw new ArgumentException("Tipo deve ser uma interface", nameof(openGenericInterface));

        return AddClasses(type =>
            IsValidClassCore(type)
            && (
                openGenericInterface.IsGenericTypeDefinition
                    ? ImplementsOpenGenericInterface(type, openGenericInterface)
                    : IsAssignableFromCached(openGenericInterface, type)
            )
        );
    }

    public IRegistrationStrategy AddClassesInheriting<TBase>()
    {
        var baseType = typeof(TBase);
        return AddClasses(type => IsValidClassCore(type) && baseType.IsAssignableFrom(type) && type != baseType);
    }

    public IRegistrationStrategy AddClassesInNamespace(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            throw new ArgumentException("Namespace não pode ser nulo ou vazio", nameof(namespaceName));

        return AddClasses(type => IsValidClassCore(type) && type.Namespace?.StartsWith(namespaceName, StringComparison.Ordinal) == true);
    }

    public IRegistrationStrategy AddClassesWithAttribute<TAttribute>()
        where TAttribute : Attribute
    {
        return AddClasses(type => IsValidClassCore(type) && type.IsDefined(typeof(TAttribute), false));
    }

    public IRegistrationStrategy AddClassesWithAutoRegisterAttribute()
    {
        EnsureAssemblies();

        var discoveredTypes = _assemblies
            .AsParallel()
            .SelectMany(GetCachedTypes)
            .Where(type => IsValidClassCore(type) && type.IsDefined(typeof(AutoRegisterAttribute), false))
            .ToArray();

        LogDiscoveredTypesIfEnabled(discoveredTypes.Length);

        foreach (var type in discoveredTypes)
        {
            var attribute = type.GetCustomAttribute<AutoRegisterAttribute>();
            if (attribute is not null)
            {
                RegisterTypeWithAttribute(type, attribute);
            }
        }

        return this;
    }

    public IRegistrationStrategy AddClassesWithNamePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern não pode ser nulo ou vazio", nameof(pattern));

        return AddClasses(type => IsValidClassCore(type) && IsMatchingPattern(type.Name, pattern));
    }

    public ITypeSelector AllowOpenGenerics()
    {
        _allowOpenGenerics = true;
        return this;
    }

    public ILifetimeConfigurator As<TService>()
    {
        _strategy = ERegistrationStrategyType.AsSpecificTypes;
        _serviceTypes = [typeof(TService)];
        return this;
    }

    public ILifetimeConfigurator As(params Type[] serviceTypes)
    {
        ArgumentNullException.ThrowIfNull(serviceTypes);
        if (serviceTypes.Length == 0)
            throw new ArgumentException("Pelo menos um tipo de serviço deve ser especificado", nameof(serviceTypes));

        _strategy = ERegistrationStrategyType.AsSpecificTypes;
        _serviceTypes = serviceTypes;
        return this;
    }

    public ILifetimeConfigurator AsImplementedInterfaces()
    {
        _strategy = ERegistrationStrategyType.AsImplementedInterfaces;
        return this;
    }

    public ILifetimeConfigurator AsSelf()
    {
        _strategy = ERegistrationStrategyType.AsSelf;
        return this;
    }

    public ITypeSelector FromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies?.Length > 0)
        {
            if (assemblies.Length > 4)
            {
                Parallel.ForEach(assemblies.Where(a => a is not null), AddAssembly);
            }
            else
            {
                foreach (var assembly in assemblies.Where(a => a is not null))
                    AddAssembly(assembly);
            }
        }
        return this;
    }

    public ITypeSelector FromAssemblyNames(params string[] assemblyNames)
    {
        if (assemblyNames?.Length > 0)
        {
            var validNames = assemblyNames.Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();

            if (validNames.Length > 4)
            {
                Parallel.ForEach(validNames, ProcessAssemblyName);
            }
            else
            {
                foreach (var name in validNames)
                    ProcessAssemblyName(name);
            }
        }
        return this;
    }

    public ITypeSelector FromAssemblyOf<T>()
    {
        AddAssembly(typeof(T).Assembly);
        return this;
    }

    public ITypeSelector FromAssemblyPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern não pode ser nulo ou vazio", nameof(pattern));

        var assemblies = AppDomain.CurrentDomain.GetAssemblies().AsParallel().Where(a => IsMatchingPattern(a.GetName().Name, pattern)).ToArray();

        if (assemblies.Length > 4)
        {
            Parallel.ForEach(assemblies, AddAssembly);
        }
        else
        {
            foreach (var assembly in assemblies)
                AddAssembly(assembly);
        }

        return this;
    }

    public ITypeSelector FromCurrentAssembly()
    {
        AddAssembly(Assembly.GetCallingAssembly());
        return this;
    }

    public ITypeSelector FromLoadedAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies().AsParallel().Where(IsNotSystemAssemblyPredicate).ToArray();

        if (assemblies.Length > 8)
        {
            Parallel.ForEach(assemblies, AddAssembly);
        }
        else
        {
            foreach (var assembly in assemblies)
                AddAssembly(assembly);
        }

        return this;
    }

    public IRegistrationResult TryAddEnumerableScoped() => RegisterAll(ServiceLifetime.Scoped, ERegistrationMode.TryAddEnumerable);

    public IRegistrationResult TryAddEnumerableSingleton() => RegisterAll(ServiceLifetime.Singleton, ERegistrationMode.TryAddEnumerable);

    public IRegistrationResult TryAddEnumerableTransient() => RegisterAll(ServiceLifetime.Transient, ERegistrationMode.TryAddEnumerable);

    public IRegistrationResult TryAddScoped() => RegisterAll(ServiceLifetime.Scoped, ERegistrationMode.TryAdd);

    public IRegistrationResult TryAddSingleton() => RegisterAll(ServiceLifetime.Singleton, ERegistrationMode.TryAdd);

    public IRegistrationResult TryAddTransient() => RegisterAll(ServiceLifetime.Transient, ERegistrationMode.TryAdd);

    public ILifetimeConfigurator UsingFactory<TService>(Func<IServiceProvider, TService> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _strategy = ERegistrationStrategyType.UsingFactory;
        _serviceTypes = [typeof(TService)];
        _factory = provider => factory(provider) ?? throw new InvalidOperationException($"Factory retornou null para {typeof(TService).Name}");
        return this;
    }

    public IRegistrationResult WithScopedLifetime() => RegisterAll(ServiceLifetime.Scoped, ERegistrationMode.Default);

    public IRegistrationResult WithSingletonLifetime() => RegisterAll(ServiceLifetime.Singleton, ERegistrationMode.Default);

    public IRegistrationResult WithTransientLifetime() => RegisterAll(ServiceLifetime.Transient, ERegistrationMode.Default);

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
    private static Type[] GetCachedTypes(Assembly assembly) =>
        AssemblyTypesCache.GetOrAdd(
            assembly,
            a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t is not null).ToArray()!;
                }
            }
        );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool HasNoExcludeAttribute(Type type) => !type.IsDefined(typeof(ExcludeFromRegistrationAttribute), false);

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
    private static bool IsAssignableFromCached(Type serviceType, Type implementationType) =>
        AssignabilityCache.GetOrAdd((serviceType, implementationType), key => IsAssignableFromCore(key.Item1, key.Item2));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAssignableFromCore(Type serviceType, Type implementationType)
    {
        if (serviceType.IsGenericTypeDefinition)
            return ImplementsOpenGenericInterface(implementationType, serviceType);
        return serviceType.IsAssignableFrom(implementationType);
    }

    private static bool IsMatchingPattern(string? name, string? pattern)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pattern))
            return false;

        if (pattern.Contains('*', StringComparison.Ordinal))
        {
            var regex = RegexCache.GetOrAdd(
                pattern,
                p =>
                {
                    var regexPattern = $"^{Regex.Escape(p).Replace("\\*", ".*", StringComparison.Ordinal)}$";
                    return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                }
            );
            return regex.IsMatch(name);
        }

        return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return string.IsNullOrEmpty(name) || SystemNamespaces.Any(ns => name.StartsWith(ns, StringComparison.Ordinal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSystemType(Type type)
    {
        var ns = type.Namespace;
        return !string.IsNullOrEmpty(ns) && SystemNamespaces.Any(sysNs => ns.StartsWith(sysNs, StringComparison.Ordinal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidClassCore(Type type) =>
        type.IsClass && !type.IsAbstract && !type.IsInterface && !type.IsNested && type.IsPublic && !IsSystemType(type);

    private void AddAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        lock (_assemblyLock)
        {
            if (_assemblies.Add(assembly))
            {
                LogAssemblyAddedIfEnabled(assembly.GetName().Name ?? "Unknown");
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddDescriptor(ServiceDescriptor descriptor)
    {
        switch (_registrationMode)
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
                throw new ArgumentOutOfRangeException(nameof(descriptor), _registrationMode, "Modo de registro inválido");
        }
    }

    private void EnsureAssemblies()
    {
        if (_assemblies.Count == 0)
            throw new InvalidOperationException("Nenhum assembly foi especificado. Use FromAssemblyOf<T>() ou FromAssemblies() primeiro.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsValidClass(Type type) => IsValidClassCore(type) && (_allowOpenGenerics || !type.IsGenericTypeDefinition);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogAssemblyAddedIfEnabled(string assemblyName)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            LogAssemblyAdded(_logger, assemblyName, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogAssemblyLoadFailedIfEnabled(string name, string message, Exception ex)
    {
        if (_logger?.IsEnabled(LogLevel.Error) == true)
            LogAssemblyLoadFailed(_logger, name, message, ex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogAssemblyNotFoundIfEnabled(string name)
    {
        if (_logger?.IsEnabled(LogLevel.Warning) == true)
            LogAssemblyNotFound(_logger, name, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogDebugTypeRegisteredIfEnabled(string typeName, string serviceTypeName)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            LogDebugTypeRegistered(_logger, typeName, serviceTypeName, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogDebugTypeRegisteredSelfIfEnabled(string typeName)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            LogDebugTypeRegisteredSelf(_logger, typeName, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogDiscoveredTypesIfEnabled(int count)
    {
        if (_logger?.IsEnabled(LogLevel.Information) == true)
            LogDiscoveredTypes(_logger, count, null);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogErrorRegisteringTypeIfEnabled(string message, string typeName, Exception ex)
    {
        if (_logger?.IsEnabled(LogLevel.Error) == true)
            LogErrorRegisteringType(_logger, message, typeName, ex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LogNoTypesDiscoveredIfEnabled(string message)
    {
        if (_logger?.IsEnabled(LogLevel.Information) == true)
            LogNoTypesDiscovered(_logger, message, null);
    }

    private void ProcessAssemblyName(string name)
    {
        try
        {
            var assembly = AssemblyCache.GetOrLoad(name);
            if (assembly is not null)
            {
                AddAssembly(assembly);
            }
            else
            {
                LogAssemblyNotFoundIfEnabled(name);
            }
        }
        catch (Exception ex)
        {
            LogAssemblyLoadFailedIfEnabled(name, ex.Message, ex);
        }
    }

    private RegistrationResult RegisterAll(ServiceLifetime lifetime, ERegistrationMode mode)
    {
        if (_typesToRegister is null || _typesToRegister.Count == 0)
        {
            LogNoTypesDiscoveredIfEnabled("Nenhum tipo foi descoberto para registro. Verifique os filtros aplicados.");
            return new RegistrationResult(EmptyTypes, TimeSpan.Zero);
        }

        _stopwatch.Restart();
        _registrationMode = mode;
        var registeredTypes = new List<Type>(_typesToRegister.Count);

        var typesSpan = CollectionsMarshal.AsSpan(_typesToRegister);
        for (int i = 0; i < typesSpan.Length; i++)
        {
            var implementationType = typesSpan[i];
            try
            {
                if (RegisterType(implementationType, lifetime))
                    registeredTypes.Add(implementationType);
            }
            catch (Exception ex)
            {
                LogErrorRegisteringTypeIfEnabled(ex.Message, implementationType.Name, ex);
                throw;
            }
        }

        _stopwatch.Stop();
        LogDiscoveredTypesIfEnabled(registeredTypes.Count);
        return new RegistrationResult(registeredTypes.AsReadOnly(), _stopwatch.Elapsed);
    }

    private bool RegisterAsImplementedInterfaces(Type implementationType, ServiceLifetime lifetime)
    {
        var interfaces = GetCachedImplementedInterfaces(implementationType);

        if (interfaces.Length == 0)
            return RegisterAsSelf(implementationType, lifetime);

        foreach (var serviceType in interfaces)
        {
            AddDescriptor(new ServiceDescriptor(serviceType, implementationType, lifetime));
            LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
        }

        return true;
    }

    private bool RegisterAsSelf(Type implementationType, ServiceLifetime lifetime)
    {
        AddDescriptor(new ServiceDescriptor(implementationType, implementationType, lifetime));
        LogDebugTypeRegisteredSelfIfEnabled(implementationType.Name);
        return true;
    }

    private bool RegisterAsSpecificTypes(Type implementationType, ServiceLifetime lifetime)
    {
        if (_serviceTypes is null || _serviceTypes.Length == 0)
            throw new InvalidOperationException("Tipos de serviço não foram especificados.");

        var registered = false;
        foreach (var serviceType in _serviceTypes)
        {
            if (IsAssignableFromCached(serviceType, implementationType))
            {
                AddDescriptor(new ServiceDescriptor(serviceType, implementationType, lifetime));
                LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
                registered = true;
            }
        }

        return registered;
    }

    private bool RegisterType(Type implementationType, ServiceLifetime lifetime)
    {
        var autoRegisterAttr = implementationType.GetCustomAttribute<AutoRegisterAttribute>();
        if (autoRegisterAttr is not null)
            return RegisterTypeWithAttribute(implementationType, autoRegisterAttr);

        return _strategy switch
        {
            ERegistrationStrategyType.AsImplementedInterfaces => RegisterAsImplementedInterfaces(implementationType, lifetime),
            ERegistrationStrategyType.AsSelf => RegisterAsSelf(implementationType, lifetime),
            ERegistrationStrategyType.AsSpecificTypes => RegisterAsSpecificTypes(implementationType, lifetime),
            ERegistrationStrategyType.UsingFactory => RegisterUsingFactory(implementationType, lifetime),
            _ => false,
        };
    }

    private bool RegisterTypeWithAttribute(Type implementationType, AutoRegisterAttribute attribute)
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

                    AddDescriptor(new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime));
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
                    AddDescriptor(new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime));
                    LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
                    registered = true;
                }

                if (interfaces.Length == 0 && attribute.AsSelf)
                {
                    AddDescriptor(new ServiceDescriptor(implementationType, implementationType, attribute.Lifetime));
                    LogDebugTypeRegisteredSelfIfEnabled(implementationType.Name);
                    registered = true;
                }
            }

            if (attribute.AsSelf)
            {
                AddDescriptor(new ServiceDescriptor(implementationType, implementationType, attribute.Lifetime));
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

    private bool RegisterUsingFactory(Type implementationType, ServiceLifetime lifetime)
    {
        if (_factory is null || _serviceTypes is null || _serviceTypes.Length == 0)
            throw new InvalidOperationException("Factory ou tipos de serviço não foram especificados.");

        foreach (var serviceType in _serviceTypes)
        {
            AddDescriptor(new ServiceDescriptor(serviceType, _factory, lifetime));
            LogDebugTypeRegisteredIfEnabled(implementationType.Name, serviceType.Name);
        }

        return true;
    }
}
