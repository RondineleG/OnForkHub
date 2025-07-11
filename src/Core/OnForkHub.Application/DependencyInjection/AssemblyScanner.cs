using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Interfaces.DependencyInjection;

using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class AssemblyScanner : IAssemblyScanner, ITypeSelector, IRegistrationStrategy, ILifetimeConfigurator
{
    public AssemblyScanner(IServiceCollection services, ILogger<AssemblyScanner>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
        _logger = logger;
    }

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

    private readonly HashSet<Assembly> _assemblies = new();

    private readonly ILogger<AssemblyScanner>? _logger;

    private readonly IServiceCollection _services;

    private readonly Stopwatch _stopwatch = new();

    private readonly List<Type> _typesToRegister = new();

    private bool _allowOpenGenerics;

    private Func<IServiceProvider, object>? _factory;

    private RegistrationMode _registrationMode = RegistrationMode.Default;

    private Type[]? _serviceTypes;

    private RegistrationStrategyType _strategy = RegistrationStrategyType.AsImplementedInterfaces;

    public IRegistrationStrategy AddClasses(Func<Type, bool>? predicate = null)
    {
        EnsureAssemblies();
        predicate ??= IsValidClass;

        var discovered = _assemblies
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .SelectMany(assembly => AssemblyCache.GetTypes(assembly))
            .Where(predicate)
            .Where(t => t.GetCustomAttribute<ExcludeFromRegistrationAttribute>() == null)
            .ToList();

        if (_logger is not null)
            LogDiscoveredTypes(_logger, discovered.Count, null);

        _typesToRegister.AddRange(discovered);
        return this;
    }

    public IRegistrationStrategy AddClassesImplementing<TInterface>()
    {
        var interfaceType = typeof(TInterface);
        return AddClasses(type => IsValidClass(type) && IsAssignableFrom(interfaceType, type));
    }

    public IRegistrationStrategy AddClassesImplementing(Type openGenericInterface)
    {
        ArgumentNullException.ThrowIfNull(openGenericInterface);

        if (!openGenericInterface.IsInterface)
            throw new ArgumentException("Tipo deve ser uma interface", nameof(openGenericInterface));

        return AddClasses(type =>
            IsValidClass(type)
            && (
                openGenericInterface.IsGenericTypeDefinition
                    ? ImplementsOpenGenericInterface(type, openGenericInterface)
                    : IsAssignableFrom(openGenericInterface, type)
            )
        );
    }

    public IRegistrationStrategy AddClassesInheriting<TBase>()
    {
        var baseType = typeof(TBase);
        return AddClasses(type => IsValidClass(type) && baseType.IsAssignableFrom(type) && type != baseType);
    }

    public IRegistrationStrategy AddClassesInNamespace(string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            throw new ArgumentException("Namespace não pode ser nulo ou vazio", nameof(namespaceName));

        return AddClasses(type => IsValidClass(type) && (type.Namespace?.StartsWith(namespaceName, StringComparison.Ordinal) == true));
    }

    public IRegistrationStrategy AddClassesWithAttribute<TAttribute>()
        where TAttribute : Attribute
    {
        return AddClasses(type => IsValidClass(type) && type.GetCustomAttribute<TAttribute>() != null);
    }

    public IRegistrationStrategy AddClassesWithAutoRegisterAttribute()
    {
        EnsureAssemblies();

        var discoveredTypes = _assemblies
            .AsParallel()
            .SelectMany(assembly => AssemblyCache.GetTypes(assembly))
            .Where(type => IsValidClass(type) && type.GetCustomAttribute<AutoRegisterAttribute>() != null)
            .ToList();

        if (_logger is not null)
            LogDiscoveredTypes(_logger, discoveredTypes.Count, null);

        foreach (var type in discoveredTypes)
        {
            var attribute = type.GetCustomAttribute<AutoRegisterAttribute>()!;
            RegisterTypeWithAttribute(type, attribute);
        }

        return this;
    }

    public IRegistrationStrategy AddClassesWithNamePattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern não pode ser nulo ou vazio", nameof(pattern));

        return AddClasses(type => IsValidClass(type) && IsMatchingPattern(type.Name, pattern));
    }

    public ITypeSelector AllowOpenGenerics()
    {
        _allowOpenGenerics = true;
        return this;
    }

    public ILifetimeConfigurator As<TService>()
    {
        _strategy = RegistrationStrategyType.AsSpecificTypes;
        _serviceTypes = new[] { typeof(TService) };
        return this;
    }

    public ILifetimeConfigurator As(params Type[] serviceTypes)
    {
        ArgumentNullException.ThrowIfNull(serviceTypes);
        if (serviceTypes.Length == 0)
            throw new ArgumentException("Pelo menos um tipo de serviço deve ser especificado", nameof(serviceTypes));

        _strategy = RegistrationStrategyType.AsSpecificTypes;
        _serviceTypes = serviceTypes;
        return this;
    }

    public ILifetimeConfigurator AsImplementedInterfaces()
    {
        _strategy = RegistrationStrategyType.AsImplementedInterfaces;
        return this;
    }

    public ILifetimeConfigurator AsSelf()
    {
        _strategy = RegistrationStrategyType.AsSelf;
        return this;
    }

    public ITypeSelector FromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies?.Length > 0)
        {
            foreach (var assembly in assemblies)
            {
                AddAssembly(assembly);
            }
        }
        return this;
    }

    public ITypeSelector FromAssemblyNames(params string[] assemblyNames)
    {
        if (assemblyNames?.Length > 0)
        {
            foreach (var name in assemblyNames)
            {
                try
                {
                    var assembly = AssemblyCache.GetOrLoad(name);
                    if (assembly != null)
                    {
                        AddAssembly(assembly);
                    }
                    else if (_logger is not null)
                    {
                        LogAssemblyNotFound(_logger, name, null);
                    }
                }
                catch (Exception ex)
                {
                    if (_logger is not null)
                        LogAssemblyLoadFailed(_logger, name, ex.Message, ex);
                }
            }
        }
        return this;
    }

    public ITypeSelector FromAssemblyOf<T>()
    {
        var assembly = typeof(T).Assembly;
        AddAssembly(assembly);
        return this;
    }

    public ITypeSelector FromAssemblyPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern não pode ser nulo ou vazio", nameof(pattern));

        var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => IsMatchingPattern(a.GetName().Name, pattern)).ToArray();

        foreach (var assembly in assemblies)
        {
            AddAssembly(assembly);
        }

        return this;
    }

    public ITypeSelector FromCurrentAssembly()
    {
        var current = Assembly.GetCallingAssembly();
        AddAssembly(current);
        return this;
    }

    public ITypeSelector FromLoadedAssemblies()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            if (!IsSystemAssembly(assembly))
            {
                AddAssembly(assembly);
            }
        }
        return this;
    }

    public IRegistrationResult TryAddEnumerableScoped() => RegisterAll(ServiceLifetime.Scoped, RegistrationMode.TryAddEnumerable);

    public IRegistrationResult TryAddEnumerableSingleton() => RegisterAll(ServiceLifetime.Singleton, RegistrationMode.TryAddEnumerable);

    public IRegistrationResult TryAddEnumerableTransient() => RegisterAll(ServiceLifetime.Transient, RegistrationMode.TryAddEnumerable);

    public IRegistrationResult TryAddScoped() => RegisterAll(ServiceLifetime.Scoped, RegistrationMode.TryAdd);

    public IRegistrationResult TryAddSingleton() => RegisterAll(ServiceLifetime.Singleton, RegistrationMode.TryAdd);

    public IRegistrationResult TryAddTransient() => RegisterAll(ServiceLifetime.Transient, RegistrationMode.TryAdd);

    public ILifetimeConfigurator UsingFactory<TService>(Func<IServiceProvider, TService> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);

        _strategy = RegistrationStrategyType.UsingFactory;
        _serviceTypes = new[] { typeof(TService) };
        _factory = provider => factory(provider)!;
        return this;
    }

    public IRegistrationResult WithScopedLifetime() => RegisterAll(ServiceLifetime.Scoped, RegistrationMode.Default);

    public IRegistrationResult WithSingletonLifetime() => RegisterAll(ServiceLifetime.Singleton, RegistrationMode.Default);

    public IRegistrationResult WithTransientLifetime() => RegisterAll(ServiceLifetime.Transient, RegistrationMode.Default);

    private static List<Type> GetImplementedInterfaces(Type type)
    {
        return type.GetInterfaces().Where(i => i != typeof(IDisposable) && i != typeof(IAsyncDisposable) && !IsSystemType(i)).ToList();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ImplementsOpenGenericInterface(Type type, Type openGenericInterface)
    {
        return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericInterface);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsAssignableFrom(Type serviceType, Type implementationType)
    {
        if (serviceType.IsGenericTypeDefinition)
        {
            return implementationType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == serviceType);
        }

        return serviceType.IsAssignableFrom(implementationType);
    }

    private static bool IsMatchingPattern(string? name, string? pattern)
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(pattern))
            return false;

        if (pattern.Contains('*'))
        {
            var regexPattern = $"^{Regex.Escape(pattern).Replace("\\*", ".*")}$";

            return Regex.IsMatch(name, regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return name?.StartsWith("System", StringComparison.Ordinal) == true
            || name?.StartsWith("Microsoft", StringComparison.Ordinal) == true
            || name?.StartsWith("mscorlib", StringComparison.Ordinal) == true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSystemType(Type type) =>
        type.Namespace?.StartsWith("System", StringComparison.Ordinal) == true
        || type.Namespace?.StartsWith("Microsoft", StringComparison.Ordinal) == true;

    private void AddAssembly(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        if (_assemblies.Add(assembly) && _logger is not null)
        {
            LogAssemblyAdded(_logger, assembly.GetName().Name ?? string.Empty, null);
        }
    }

    private void AddDescriptor(ServiceDescriptor descriptor)
    {
        switch (_registrationMode)
        {
            case RegistrationMode.Default:
                _services.Add(descriptor);
                break;

            case RegistrationMode.TryAdd:
                _services.TryAdd(descriptor);
                break;

            case RegistrationMode.TryAddEnumerable:
                _services.TryAddEnumerable(descriptor);
                break;
        }
    }

    private void EnsureAssemblies()
    {
        if (_assemblies.Count == 0)
            throw new InvalidOperationException("Nenhum assembly foi especificado. Use FromAssemblyOf<T>() ou FromAssemblies() primeiro.");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsValidClass(Type type) =>
        type.IsClass
        && !type.IsAbstract
        && !type.IsInterface
        && !type.IsNested
        && type.IsPublic
        && (_allowOpenGenerics || !type.IsGenericTypeDefinition)
        && !IsSystemType(type);

    private RegistrationResult RegisterAll(ServiceLifetime lifetime, RegistrationMode mode)
    {
        if (_typesToRegister.Count == 0)
        {
            if (_logger is not null)
                Console.WriteLine("Nenhum tipo foi descoberto para registro. Verifique os filtros aplicados.");
            return new RegistrationResult(Array.Empty<Type>(), TimeSpan.Zero);
        }

        _stopwatch.Start();
        _registrationMode = mode;
        var registeredTypes = new List<Type>();

        foreach (var implementationType in _typesToRegister)
        {
            try
            {
                if (RegisterType(implementationType, lifetime))
                {
                    registeredTypes.Add(implementationType);
                }
            }
            catch (Exception ex)
            {
                if (_logger is not null)
                    LogErrorRegisteringType(_logger, ex.Message, implementationType.Name, ex);
                throw;
            }
        }

        _stopwatch.Stop();

        if (_logger is not null)
            LogDiscoveredTypes(_logger, registeredTypes.Count, null);

        return new RegistrationResult(registeredTypes.AsReadOnly(), _stopwatch.Elapsed);
    }

    private bool RegisterAsImplementedInterfaces(Type implementationType, ServiceLifetime lifetime)
    {
        var interfaces = GetImplementedInterfaces(implementationType);

        if (interfaces.Count == 0)
        {
            return RegisterAsSelf(implementationType, lifetime);
        }

        foreach (var serviceType in interfaces)
        {
            var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
            AddDescriptor(descriptor);
            if (_logger is not null)
                LogDebugTypeRegistered(_logger, implementationType.Name, serviceType.Name, null);
        }

        return true;
    }

    private bool RegisterAsSelf(Type implementationType, ServiceLifetime lifetime)
    {
        var descriptor = new ServiceDescriptor(implementationType, implementationType, lifetime);
        AddDescriptor(descriptor);
        if (_logger is not null)
            LogDebugTypeRegisteredSelf(_logger, implementationType.Name, null);
        return true;
    }

    private bool RegisterAsSpecificTypes(Type implementationType, ServiceLifetime lifetime)
    {
        if (_serviceTypes == null || _serviceTypes.Length == 0)
            throw new InvalidOperationException("Tipos de serviço não foram especificados.");

        var registered = false;
        foreach (var serviceType in _serviceTypes)
        {
            if (IsAssignableFrom(serviceType, implementationType))
            {
                var descriptor = new ServiceDescriptor(serviceType, implementationType, lifetime);
                AddDescriptor(descriptor);
                if (_logger is not null)
                    LogDebugTypeRegistered(_logger, implementationType.Name, serviceType.Name, null);
                registered = true;
            }
            else if (_logger is not null)
            {
                LogAssemblyNotFound(_logger, serviceType.Name, null);
            }
        }

        return registered;
    }

    private bool RegisterType(Type implementationType, ServiceLifetime lifetime)
    {
        var autoRegisterAttr = implementationType.GetCustomAttribute<AutoRegisterAttribute>();
        if (autoRegisterAttr != null)
        {
            return RegisterTypeWithAttribute(implementationType, autoRegisterAttr);
        }

        return _strategy switch
        {
            RegistrationStrategyType.AsImplementedInterfaces => RegisterAsImplementedInterfaces(implementationType, lifetime),
            RegistrationStrategyType.AsSelf => RegisterAsSelf(implementationType, lifetime),
            RegistrationStrategyType.AsSpecificTypes => RegisterAsSpecificTypes(implementationType, lifetime),
            RegistrationStrategyType.UsingFactory => RegisterUsingFactory(implementationType, lifetime),
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
                    if (!IsAssignableFrom(serviceType, implementationType))
                    {
                        if (_logger is not null)
                            LogAssemblyNotFound(_logger, serviceType.Name, null);
                        continue;
                    }

                    var descriptor = new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime);
                    AddDescriptor(descriptor);
                    if (_logger is not null)
                        LogDebugTypeRegistered(_logger, implementationType.Name, serviceType.Name, null);
                    registered = true;
                }
                return registered;
            }

            if (attribute.AsImplementedInterfaces)
            {
                var interfaces = GetImplementedInterfaces(implementationType);

                foreach (var serviceType in interfaces)
                {
                    var descriptor = new ServiceDescriptor(serviceType, implementationType, attribute.Lifetime);
                    AddDescriptor(descriptor);
                    if (_logger is not null)
                        LogDebugTypeRegistered(_logger, implementationType.Name, serviceType.Name, null);
                    registered = true;
                }

                if (interfaces.Count == 0 && attribute.AsSelf)
                {
                    var selfDescriptor = new ServiceDescriptor(implementationType, implementationType, attribute.Lifetime);
                    AddDescriptor(selfDescriptor);
                    if (_logger is not null)
                        LogDebugTypeRegisteredSelf(_logger, implementationType.Name, null);
                    registered = true;
                }
            }

            if (attribute.AsSelf)
            {
                var selfDescriptor = new ServiceDescriptor(implementationType, implementationType, attribute.Lifetime);
                AddDescriptor(selfDescriptor);
                if (_logger is not null)
                    LogDebugTypeRegisteredSelf(_logger, implementationType.Name, null);
                registered = true;
            }

            return registered;
        }
        catch (Exception ex)
        {
            if (_logger is not null)
                LogErrorRegisteringType(_logger, ex.Message, implementationType.Name, ex);
            throw;
        }
    }

    private bool RegisterUsingFactory(Type implementationType, ServiceLifetime lifetime)
    {
        if (_factory == null || _serviceTypes == null || _serviceTypes.Length == 0)
            throw new InvalidOperationException("Factory ou tipos de serviço não foram especificados.");

        foreach (var serviceType in _serviceTypes)
        {
            var descriptor = new ServiceDescriptor(serviceType, _factory, lifetime);
            AddDescriptor(descriptor);
            if (_logger is not null)
                LogDebugTypeRegistered(_logger, implementationType.Name, serviceType.Name, null);
        }

        return true;
    }
}
