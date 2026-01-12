using Microsoft.Extensions.Logging;
using OnForkHub.Core.Attributes;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class TypeSelector
{
    public TypeSelector(ILogger? logger = null)
    {
        _logger = logger;
    }

    private static readonly ConcurrentDictionary<(Type, Type), bool> AssignabilityCache = new();

    private static readonly Action<ILogger, int, Exception?> LogDiscoveredTypes = LoggerMessage.Define<int>(
        LogLevel.Information,
        new EventId(1, nameof(LogDiscoveredTypes)),
        "Discovered {Count} types for registration"
    );

    private static readonly FrozenSet<string> SystemNamespaces = new[] { "System", "Microsoft", "mscorlib" }.ToFrozenSet(StringComparer.Ordinal);

    private readonly ILogger? _logger;

    private readonly List<Type> _typesToRegister = new();

    private bool _allowOpenGenerics;

    public IReadOnlyList<Type> TypesToRegister => _typesToRegister.AsReadOnly();

    public void AddClasses(IReadOnlySet<Assembly> assemblies, Func<Type, bool>? predicate = null)
    {
        EnsureAssemblies(assemblies);
        predicate ??= IsValidClassCore;

        var discovered = assemblies
            .AsParallel()
            .WithDegreeOfParallelism(Environment.ProcessorCount)
            .SelectMany(AssemblyCache.GetTypes)
            .Where(predicate)
            .Where(HasNoExcludeAttribute)
            .ToList();

        LogDiscoveredTypesIfEnabled(discovered.Count);
        _typesToRegister.AddRange(discovered);
    }

    public void AddClassesImplementing<TInterface>(IReadOnlySet<Assembly> assemblies)
    {
        var interfaceType = typeof(TInterface);
        AddClasses(assemblies, type => IsValidClassCore(type) && IsAssignableFromCached(interfaceType, type));
    }

    public void AddClassesImplementing(IReadOnlySet<Assembly> assemblies, Type openGenericInterface)
    {
        ArgumentNullException.ThrowIfNull(openGenericInterface);
        if (!openGenericInterface.IsInterface)
            throw new ArgumentException("Type must be an interface", nameof(openGenericInterface));

        AddClasses(
            assemblies,
            type =>
                IsValidClassCore(type)
                && (
                    openGenericInterface.IsGenericTypeDefinition
                        ? ImplementsOpenGenericInterface(type, openGenericInterface)
                        : IsAssignableFromCached(openGenericInterface, type)
                )
        );
    }

    public void AddClassesInheriting<TBase>(IReadOnlySet<Assembly> assemblies)
    {
        var baseType = typeof(TBase);
        AddClasses(assemblies, type => IsValidClassCore(type) && baseType.IsAssignableFrom(type) && type != baseType);
    }

    public void AddClassesInNamespace(IReadOnlySet<Assembly> assemblies, string namespaceName)
    {
        if (string.IsNullOrWhiteSpace(namespaceName))
            throw new ArgumentException("Namespace cannot be null or empty", nameof(namespaceName));

        AddClasses(assemblies, type => IsValidClassCore(type) && type.Namespace?.StartsWith(namespaceName, StringComparison.Ordinal) == true);
    }

    public void AddClassesWithAttribute<TAttribute>(IReadOnlySet<Assembly> assemblies)
        where TAttribute : Attribute
    {
        AddClasses(assemblies, type => IsValidClassCore(type) && type.IsDefined(typeof(TAttribute), false));
    }

    public void AddClassesWithAutoRegisterAttribute(IReadOnlySet<Assembly> assemblies)
    {
        var discoveredTypes = assemblies
            .AsParallel()
            .SelectMany(AssemblyCache.GetTypes)
            .Where(type => IsValidClassCore(type) && type.IsDefined(typeof(AutoRegisterAttribute), false))
            .ToArray();

        LogDiscoveredTypesIfEnabled(discoveredTypes.Length);
        _typesToRegister.AddRange(discoveredTypes);
    }

    public void AddClassesWithNamePattern(IReadOnlySet<Assembly> assemblies, string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

        AddClasses(assemblies, type => IsValidClassCore(type) && IsMatchingPattern(type.Name, pattern));
    }

    public void AllowOpenGenerics()
    {
        _allowOpenGenerics = true;
    }

    private static void EnsureAssemblies(IReadOnlySet<Assembly> assemblies)
    {
        if (assemblies.Count == 0)
            throw new InvalidOperationException("No assembly has been specified. Use FromAssemblyOf<T>() or FromAssemblies() first.");
    }

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
            var regexPattern = $"^{System.Text.RegularExpressions.Regex.Escape(pattern).Replace("\\*", ".*", StringComparison.Ordinal)}$";
            var regex = new System.Text.RegularExpressions.Regex(
                regexPattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant
            );
            return regex.IsMatch(name);
        }

        return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSystemType(Type type)
    {
        var ns = type.Namespace;
        return !string.IsNullOrEmpty(ns) && SystemNamespaces.Any(sysNs => ns.StartsWith(sysNs, StringComparison.Ordinal));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsValidClassCore(Type type) =>
        type.IsClass
        && !type.IsAbstract
        && !type.IsInterface
        && !type.IsNested
        && type.IsPublic
        && !type.IsGenericTypeDefinition
        && !IsSystemType(type);

    private bool IsValidClass(Type type) => IsValidClassCore(type) && (_allowOpenGenerics || !type.IsGenericTypeDefinition);

    private void LogDiscoveredTypesIfEnabled(int count)
    {
        if (_logger?.IsEnabled(LogLevel.Information) == true)
            LogDiscoveredTypes(_logger, count, null);
    }
}
