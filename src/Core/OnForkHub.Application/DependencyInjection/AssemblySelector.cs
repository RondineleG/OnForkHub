using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Text.RegularExpressions;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class AssemblySelector
{
    public AssemblySelector(ILogger? logger = null)
    {
        _logger = logger;
    }

    private static readonly Func<Assembly, bool> IsNotSystemAssemblyPredicate = assembly => !IsSystemAssembly(assembly);

    private static readonly Action<ILogger, string, Exception?> LogAssemblyAdded = LoggerMessage.Define<string>(
        LogLevel.Debug,
        new EventId(2, nameof(LogAssemblyAdded)),
        "Assembly added: {Assembly}"
    );

    private static readonly Action<ILogger, string, string, Exception?> LogAssemblyLoadFailed = LoggerMessage.Define<string, string>(
        LogLevel.Error,
        new EventId(2, nameof(LogAssemblyLoadFailed)),
        "Failure when loading assembly '{AssemblyName}': {ErrorMessage}"
    );

    private static readonly Action<ILogger, string, Exception?> LogAssemblyNotFound = LoggerMessage.Define<string>(
        LogLevel.Warning,
        new EventId(3, nameof(LogAssemblyNotFound)),
        "Assembly not found: {AssemblyName}"
    );

    private static readonly ConcurrentDictionary<string, Regex> RegexCache = new(Environment.ProcessorCount, 100);

    private static readonly FrozenSet<string> SystemNamespaces = new[] { "System", "Microsoft", "mscorlib" }.ToFrozenSet(StringComparer.Ordinal);

    private readonly HashSet<Assembly> _assemblies = new(ReferenceEqualityComparer.Instance);

    private readonly object _assemblyLock = new();

    private readonly ILogger? _logger;

    public IReadOnlySet<Assembly> Assemblies => _assemblies.ToHashSet();

    public void FromAssemblies(params Assembly[] assemblies)
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
    }

    public void FromAssemblyNames(params string[] assemblyNames)
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
    }

    public void FromAssemblyOf<T>()
    {
        AddAssembly(typeof(T).Assembly);
    }

    public void FromAssemblyPattern(string pattern)
    {
        if (string.IsNullOrWhiteSpace(pattern))
            throw new ArgumentException("Pattern cannot be null or empty", nameof(pattern));

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
    }

    public void FromCurrentAssembly()
    {
        AddAssembly(Assembly.GetCallingAssembly());
    }

    public void FromLoadedAssemblies()
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
                    var escapedPattern = Regex.Escape(p).Replace("\\*", ".*", StringComparison.Ordinal);
                    var regexPattern = $"^{escapedPattern}$";
                    return new Regex(regexPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);
                }
            );
            return regex.IsMatch(name);
        }

        return name.Contains(pattern, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSystemAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        return string.IsNullOrEmpty(name) || SystemNamespaces.Any(ns => name.StartsWith(ns, StringComparison.Ordinal));
    }

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

    private void LogAssemblyAddedIfEnabled(string assemblyName)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
            LogAssemblyAdded(_logger, assemblyName, null);
    }

    private void LogAssemblyLoadFailedIfEnabled(string name, string message, Exception ex)
    {
        if (_logger?.IsEnabled(LogLevel.Error) == true)
            LogAssemblyLoadFailed(_logger, name, message, ex);
    }

    private void LogAssemblyNotFoundIfEnabled(string name)
    {
        if (_logger?.IsEnabled(LogLevel.Warning) == true)
            LogAssemblyNotFound(_logger, name, null);
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
}
