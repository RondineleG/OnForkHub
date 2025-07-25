using Microsoft.Extensions.DependencyInjection;
using OnForkHub.CrossCutting.DependencyInjection;

namespace OnForkHub.CrossCutting.Test;

public class TestService : ITestService
{
    public string GetMessage() => "Test service implementation";
}

public interface ITestService
{
    string GetMessage();
}

public class DependencyInjectionTests
{
    public static void TestRegistrationStrategy()
    {
        var services = new ServiceCollection();
        var typesToRegister = new[] { typeof(TestService) };
        
        var strategy = new RegistrationStrategy(typesToRegister, ServiceLifetime.Scoped);
        strategy.Register(services);
        
        var serviceProvider = services.BuildServiceProvider();
        var testService = serviceProvider.GetService<ITestService>();
        
        if (testService?.GetMessage() == "Test service implementation")
        {
            Console.WriteLine("✓ RegistrationStrategy test passed");
        }
        else
        {
            Console.WriteLine("✗ RegistrationStrategy test failed");
        }
    }

    public static void TestLifetimeConfigurator()
    {
        var configurator = new LifetimeConfigurator(ServiceLifetime.Singleton);
        
        if (configurator.GetLifetime() == ServiceLifetime.Singleton)
        {
            Console.WriteLine("✓ LifetimeConfigurator test passed");
        }
        else
        {
            Console.WriteLine("✗ LifetimeConfigurator test failed");
        }
    }

    public static void TestTypeSelector()
    {
        var typesToRegister = new[] { typeof(TestService) };
        var typeSelector = new TypeSelectorService(typesToRegister);
        var strategy = typeSelector.CreateRegistrationStrategy(ServiceLifetime.Transient);
        
        if (strategy != null)
        {
            Console.WriteLine("✓ TypeSelectorService test passed");
        }
        else
        {
            Console.WriteLine("✗ TypeSelectorService test failed");
        }
    }

    public static void TestAssemblyScanner()
    {
        var scanner = new AssemblyScanner(typeof(TestService).Assembly);
        var typeSelector = scanner.FindTypesImplementing<ITestService>();
        
        if (typeSelector != null)
        {
            Console.WriteLine("✓ AssemblyScanner test passed");
        }
        else
        {
            Console.WriteLine("✗ AssemblyScanner test failed");
        }
    }

    public static void RunAllTests()
    {
        Console.WriteLine("Running DI System Tests...");
        TestRegistrationStrategy();
        TestLifetimeConfigurator();
        TestTypeSelector();
        TestAssemblyScanner();
        Console.WriteLine("All tests completed.");
    }
}