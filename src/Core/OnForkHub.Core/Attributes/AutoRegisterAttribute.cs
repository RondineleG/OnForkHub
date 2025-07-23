using Microsoft.Extensions.DependencyInjection;

namespace OnForkHub.Application.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class AutoRegisterAttribute : Attribute
{
    public bool AsImplementedInterfaces { get; set; } = true;

    public bool AsSelf { get; set; }

    public Type[]? AsTypes { get; set; }

    public ServiceLifetime Lifetime { get; set; } = ServiceLifetime.Scoped;
}
