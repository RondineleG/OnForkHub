namespace OnForkHub.Core.Interfaces.DependencyInjection;

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
