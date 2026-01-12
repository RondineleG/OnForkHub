using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using OnForkHub.Core.Interfaces.DependencyInjection;

using System.Reflection;

namespace OnForkHub.Application.DependencyInjection;

internal sealed class AssemblyScanner : IAssemblyScanner, ITypeSelector, IRegistrationStrategy, ILifetimeConfigurator
{
    public AssemblyScanner(IServiceCollection services, ILogger<AssemblyScanner>? logger = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        _assemblySelector = new AssemblySelector(logger);
        _typeSelector = new TypeSelector(logger);
        _serviceRegistrar = new ServiceRegister(services, logger);
    }

    private readonly AssemblySelector _assemblySelector;

    private readonly ServiceRegister _serviceRegistrar;

    private readonly TypeSelector _typeSelector;

    private Func<IServiceProvider, object>? _factory;

    private Type[]? _serviceTypes;

    private ERegistrationStrategyType _strategy = ERegistrationStrategyType.AsImplementedInterfaces;

    public IRegistrationStrategy AddClasses(Func<Type, bool>? predicate = null)
    {
        _typeSelector.AddClasses(_assemblySelector.Assemblies, predicate);
        return this;
    }

    public IRegistrationStrategy AddClassesImplementing<TInterface>()
    {
        _typeSelector.AddClassesImplementing<TInterface>(_assemblySelector.Assemblies);
        return this;
    }

    public IRegistrationStrategy AddClassesImplementing(Type openGenericInterface)
    {
        _typeSelector.AddClassesImplementing(_assemblySelector.Assemblies, openGenericInterface);
        return this;
    }

    public IRegistrationStrategy AddClassesInheriting<TBase>()
    {
        _typeSelector.AddClassesInheriting<TBase>(_assemblySelector.Assemblies);
        return this;
    }

    public IRegistrationStrategy AddClassesInNamespace(string namespaceName)
    {
        _typeSelector.AddClassesInNamespace(_assemblySelector.Assemblies, namespaceName);
        return this;
    }

    public IRegistrationStrategy AddClassesWithAttribute<TAttribute>()
        where TAttribute : Attribute
    {
        _typeSelector.AddClassesWithAttribute<TAttribute>(_assemblySelector.Assemblies);
        return this;
    }

    public IRegistrationStrategy AddClassesWithAutoRegisterAttribute()
    {
        _typeSelector.AddClassesWithAutoRegisterAttribute(_assemblySelector.Assemblies);
        return this;
    }

    public IRegistrationStrategy AddClassesWithNamePattern(string pattern)
    {
        _typeSelector.AddClassesWithNamePattern(_assemblySelector.Assemblies, pattern);
        return this;
    }

    public ITypeSelector AllowOpenGenerics()
    {
        _typeSelector.AllowOpenGenerics();
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
            throw new ArgumentException("At least one service type must be specified", nameof(serviceTypes));

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
        _assemblySelector.FromAssemblies(assemblies);
        return this;
    }

    public ITypeSelector FromAssemblyNames(params string[] assemblyNames)
    {
        _assemblySelector.FromAssemblyNames(assemblyNames);
        return this;
    }

    public ITypeSelector FromAssemblyOf<T>()
    {
        _assemblySelector.FromAssemblyOf<T>();
        return this;
    }

    public ITypeSelector FromAssemblyPattern(string pattern)
    {
        _assemblySelector.FromAssemblyPattern(pattern);
        return this;
    }

    public ITypeSelector FromCurrentAssembly()
    {
        _assemblySelector.FromCurrentAssembly();
        return this;
    }

    public ITypeSelector FromLoadedAssemblies()
    {
        _assemblySelector.FromLoadedAssemblies();
        return this;
    }

    public IRegistrationResult TryAddEnumerableScoped() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Scoped,
            ERegistrationMode.TryAddEnumerable,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult TryAddEnumerableSingleton() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Singleton,
            ERegistrationMode.TryAddEnumerable,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult TryAddEnumerableTransient() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Transient,
            ERegistrationMode.TryAddEnumerable,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult TryAddScoped() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Scoped,
            ERegistrationMode.TryAdd,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult TryAddSingleton() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Singleton,
            ERegistrationMode.TryAdd,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult TryAddTransient() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Transient,
            ERegistrationMode.TryAdd,
            _serviceTypes,
            _factory
        );

    public ILifetimeConfigurator UsingFactory<TService>(Func<IServiceProvider, TService> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _strategy = ERegistrationStrategyType.UsingFactory;
        _serviceTypes = [typeof(TService)];
        _factory = provider => factory(provider) ?? throw new InvalidOperationException($"Factory returned null for {typeof(TService).Name}");
        return this;
    }

    public IRegistrationResult WithScopedLifetime() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Scoped,
            ERegistrationMode.Default,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult WithSingletonLifetime() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Singleton,
            ERegistrationMode.Default,
            _serviceTypes,
            _factory
        );

    public IRegistrationResult WithTransientLifetime() =>
        _serviceRegistrar.RegisterAll(
            _typeSelector.TypesToRegister,
            _strategy,
            ServiceLifetime.Transient,
            ERegistrationMode.Default,
            _serviceTypes,
            _factory
        );
}
