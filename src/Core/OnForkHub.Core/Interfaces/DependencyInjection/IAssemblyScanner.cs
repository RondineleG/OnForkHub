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
