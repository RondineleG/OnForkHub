using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace OnForkHub.CrossCutting.DependencyInjection;

public class AssemblyScanner
{
    private readonly Assembly[] _assemblies;

    public AssemblyScanner(params Assembly[] assemblies)
    {
        _assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
    }

    public TypeSelectorService FromAssemblies(params Assembly[] assemblies)
    {
        var allAssemblies = _assemblies.Concat(assemblies).ToArray();
        var types = allAssemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => !type.IsAbstract && type.IsClass).ToArray();

        return new TypeSelectorService(types);
    }

    public TypeSelectorService FromAssemblyOf<T>()
    {
        return FromAssemblies(typeof(T).Assembly);
    }

    public TypeSelectorService FromAssemblyOf(Type markerType)
    {
        return FromAssemblies(markerType.Assembly);
    }

    public TypeSelectorService FindTypesImplementing<T>()
    {
        return FindTypesImplementing(typeof(T));
    }

    public TypeSelectorService FindTypesImplementing(Type interfaceType)
    {
        var types = _assemblies
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type =>
                !type.IsAbstract
                && type.IsClass
                && type.GetInterfaces().Any(i => i.IsGenericType ? i.GetGenericTypeDefinition() == interfaceType : i == interfaceType)
            )
            .ToArray();

        return new TypeSelectorService(types);
    }

    public void RegisterServices(IServiceCollection services, LifetimeConfigurator configurator)
    {
        var types = _assemblies.SelectMany(assembly => assembly.GetTypes()).Where(type => !type.IsAbstract && type.IsClass).ToArray();

        var strategy = new RegistrationStrategy(types, configurator.GetLifetime());
        strategy.Register(services);
    }
}
