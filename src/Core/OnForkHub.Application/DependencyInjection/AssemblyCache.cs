using System.Collections.Concurrent;
using System.Collections.Frozen;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OnForkHub.Application.DependencyInjection;

internal static class AssemblyCache
{
    private static readonly ConcurrentDictionary<string, Assembly?> _assemblyCache = new(StringComparer.OrdinalIgnoreCase);

    private static readonly Lazy<FrozenDictionary<string, Assembly>> _loadedAssembliesCache = new(() =>
        AppDomain
            .CurrentDomain.GetAssemblies()
            .Where(static a => !string.IsNullOrEmpty(a.GetName().Name))
            .ToFrozenDictionary(static a => a.GetName().Name!, StringComparer.OrdinalIgnoreCase)
    );

    private static readonly ConcurrentDictionary<Assembly, Type[]> _typeCache = new();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Assembly? GetOrLoad(ReadOnlySpan<char> assemblyName)
    {
        var key = assemblyName.ToString();
        return _assemblyCache.GetOrAdd(key, static name => LoadAssemblyInternal(name));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type[] GetTypes(Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);
        return _typeCache.GetOrAdd(assembly, static asm => GetTypesInternal(asm));
    }

    private static Type[] GetTypesInternal(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            var validTypes = new List<Type>(ex.Types?.Length ?? 0);

            if (ex.Types is not null)
            {
                for (var i = 0; i < ex.Types.Length; i++)
                {
                    var type = ex.Types[i];
                    if (type is not null)
                        validTypes.Add(type);
                }
            }

            return validTypes.ToArray();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
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
