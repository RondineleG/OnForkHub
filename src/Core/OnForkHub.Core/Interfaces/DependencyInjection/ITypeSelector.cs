namespace OnForkHub.Core.Interfaces.DependencyInjection;

public interface IAssemblyScanner
{
    ITypeSelector FromAssemblies(params Assembly[] assemblies);

    ITypeSelector FromAssemblyNames(params string[] assemblyNames);

    ITypeSelector FromAssemblyOf<T>();

    ITypeSelector FromAssemblyPattern(string pattern);

    ITypeSelector FromCurrentAssembly();

    ITypeSelector FromLoadedAssemblies();
}

public interface ILifetimeConfigurator
{
    IRegistrationResult TryAddEnumerableScoped();

    IRegistrationResult TryAddEnumerableSingleton();

    IRegistrationResult TryAddEnumerableTransient();

    IRegistrationResult TryAddScoped();

    IRegistrationResult TryAddSingleton();

    IRegistrationResult TryAddTransient();

    IRegistrationResult WithScopedLifetime();

    IRegistrationResult WithSingletonLifetime();

    IRegistrationResult WithTransientLifetime();
}

public interface IRegistrationStrategy
{
    ILifetimeConfigurator As<TService>();

    ILifetimeConfigurator As(params Type[] serviceTypes);

    ILifetimeConfigurator AsImplementedInterfaces();

    ILifetimeConfigurator AsSelf();

    ILifetimeConfigurator UsingFactory<TService>(Func<IServiceProvider, TService> factory);
}

public interface ITypeSelector
{
    IRegistrationStrategy AddClasses(Func<Type, bool>? predicate = null);

    IRegistrationStrategy AddClassesImplementing<TInterface>();

    IRegistrationStrategy AddClassesImplementing(Type openGenericInterface);

    IRegistrationStrategy AddClassesInheriting<TBase>();

    IRegistrationStrategy AddClassesInNamespace(string namespaceName);

    IRegistrationStrategy AddClassesWithAttribute<TAttribute>()
        where TAttribute : Attribute;

    IRegistrationStrategy AddClassesWithAutoRegisterAttribute();

    IRegistrationStrategy AddClassesWithNamePattern(string pattern);

    ITypeSelector AllowOpenGenerics();
}

public interface IRegistrationResult
{
    TimeSpan ElapsedTime { get; }

    IReadOnlyList<Type> RegisteredTypes { get; }

    int RegisteredTypesCount { get; }
}
