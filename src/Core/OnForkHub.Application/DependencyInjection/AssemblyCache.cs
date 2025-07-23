using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OnForkHub.Application.DependencyInjection;

internal static class AssemblyCache
{
    private static readonly ConcurrentDictionary<string, Assembly?> _assemblyCache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<Dictionary<string, Assembly>> _loadedAssembliesCache = new(() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(a => a.GetName().Name != null)
            .ToDictionary(a => a.GetName().Name!, StringComparer.OrdinalIgnoreCase)
    );

    private static readonly ConcurrentDictionary<Assembly, Type[]> _typeCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Assembly? GetOrLoad(string assemblyName)
    {
        if (_assemblyCache.TryGetValue(assemblyName, out var cachedAssembly))
            return cachedAssembly;

        return _assemblyCache.GetOrAdd(assemblyName, LoadAssemblyInternal);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type[] GetTypes(Assembly assembly)
    {
        return _typeCache.GetOrAdd(assembly, GetTypesInternal);
    }

    private static Type[] GetTypesInternal(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var validTypes = new List<Type>();
            foreach (var type in ex.Types)
            {
                if (type != null)
                    validTypes.Add(type);
            }
            return validTypes.ToArray();
        }
    }

    private static Assembly? LoadAssemblyInternal(string assemblyName)
    {
        if (_loadedAssembliesCache.Value.TryGetValue(assemblyName, out var loadedAssembly))
            return loadedAssembly;

        try
        {
            return Assembly.Load(assemblyName);
        }
        catch
        {
            return null;
        }
    }
}
