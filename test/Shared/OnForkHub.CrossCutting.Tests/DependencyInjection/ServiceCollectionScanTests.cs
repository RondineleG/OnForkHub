using Microsoft.Extensions.DependencyInjection;

using OnForkHub.CrossCutting.DependencyInjection;
using OnForkHub.CrossCutting.Tests.DependencyInjection.Fixtures;

namespace OnForkHub.CrossCutting.Tests.DependencyInjection;

[TestClass]
[TestCategory("Unit")]
public class ServiceCollectionScanTests
{
    private static readonly System.Reflection.Assembly TestAssembly = typeof(ServiceCollectionScanTests).Assembly;

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void AddClassesImplementingAsServiceRegistersEveryConcreteImplementation()
    {
        var services = new ServiceCollection();

        services.Scan(scan => scan.FromAssemblies(TestAssembly).AddClassesImplementing<IScanGreeter>().As<IScanGreeter>().WithScopedLifetime());

        var implementations = services.Where(d => d.ServiceType == typeof(IScanGreeter)).Select(d => d.ImplementationType).ToList();
        implementations.Should().BeEquivalentTo([typeof(EnglishGreeter), typeof(AuditedGreeter)]);
        services.Should().OnlyContain(d => d.Lifetime == ServiceLifetime.Scoped);
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void ScanSkipsAbstractClassesAndTypesMarkedWithExcludeFromRegistration()
    {
        var services = new ServiceCollection();

        services.Scan(scan =>
            scan.FromAssemblies(TestAssembly).AddClassesImplementing<IScanGreeter>().AsImplementedInterfaces().WithScopedLifetime()
        );

        services.Select(d => d.ImplementationType).Should().NotContain([typeof(ExcludedGreeter), typeof(AbstractGreeter)]);
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void AsImplementedInterfacesIgnoresSystemInterfaces()
    {
        var services = new ServiceCollection();

        services.Scan(scan =>
            scan.FromAssemblies(TestAssembly).AddClasses(t => t == typeof(EnglishGreeter)).AsImplementedInterfaces().WithScopedLifetime()
        );

        services.Select(d => d.ServiceType).Should().BeEquivalentTo([typeof(IScanGreeter)]);
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void AsSelfWithInterfacesRegistersConcreteTypeAndEachInterface()
    {
        var services = new ServiceCollection();

        services.Scan(scan =>
            scan.FromAssemblies(TestAssembly).AddClasses(t => t == typeof(AuditedGreeter)).AsSelfWithInterfaces().WithTransientLifetime()
        );

        services.Select(d => d.ServiceType).Should().BeEquivalentTo([typeof(AuditedGreeter), typeof(IScanGreeter), typeof(IScanAudit)]);
        services.Should().OnlyContain(d => d.ImplementationType == typeof(AuditedGreeter) && d.Lifetime == ServiceLifetime.Transient);
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void AddClassesImplementingOpenGenericRegistersClosedInterfaces()
    {
        var services = new ServiceCollection();

        services.Scan(scan =>
            scan.FromAssemblies(TestAssembly).AddClassesImplementing(typeof(IScanHandler<>)).AsImplementedInterfaces().WithScopedLifetime()
        );

        using var provider = services.BuildServiceProvider();
        provider.GetRequiredService<IScanHandler<string>>().Should().BeOfType<StringScanHandler>();
        provider.GetRequiredService<IScanHandler<int>>().Should().BeOfType<IntScanHandler>();
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void AutoRegisterAttributeAsSelfRegistersTypeOnceWithAttributeLifetime()
    {
        var services = new ServiceCollection();

        services.AddAutoRegisteredServices(TestAssembly);

        var descriptor = services.Should().ContainSingle(d => d.ServiceType == typeof(SelfOnlyClock)).Subject;
        descriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void TryAddScopedKeepsExistingRegistration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IScanGreeter, AuditedGreeter>();

        services.Scan(scan => scan.FromAssemblies(TestAssembly).AddClasses(t => t == typeof(EnglishGreeter)).As<IScanGreeter>().TryAddScoped());

        services.Should().ContainSingle(d => d.ServiceType == typeof(IScanGreeter)).Which.ImplementationType.Should().Be<AuditedGreeter>();
    }

    [TestMethod]
    [TestCategory("DependencyInjection")]
    public void ScanWithoutAssembliesThrows()
    {
        var services = new ServiceCollection();

        var act = () => services.Scan(scan => scan.FromAssemblies().AddClasses());

        act.Should().Throw<InvalidOperationException>();
    }
}
