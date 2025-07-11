using System.Collections.Concurrent;
using System.Reflection;

namespace OnForkHub.Application.DependencyInjection;

internal static class AssemblyCache
{
    private static readonly ConcurrentDictionary<string, Assembly?> _assemblyCache = new();

    private static readonly object _lock = new();

    private static readonly ConcurrentDictionary<Assembly, Type[]> _typeCache = new();

    public static void ClearCache()
    {
        _assemblyCache.Clear();
        _typeCache.Clear();
    }

    public static Assembly? GetOrLoad(string assemblyName)
    {
        return _assemblyCache.GetOrAdd(
            assemblyName,
            name =>
            {
                lock (_lock)
                {
                    var loaded = AppDomain
                        .CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));

                    if (loaded != null)
                        return loaded;

                    try
                    {
                        return Assembly.Load(name);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        );
    }

    public static Type[] GetTypes(Assembly assembly)
    {
        return _typeCache.GetOrAdd(
            assembly,
            asm =>
            {
                try
                {
                    return asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    return ex.Types.Where(t => t != null).ToArray()!;
                }
            }
        );
    }
}
