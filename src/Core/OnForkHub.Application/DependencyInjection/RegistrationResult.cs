using OnForkHub.Core.Interfaces.DependencyInjection;

namespace OnForkHub.Application.DependencyInjection;

public class RegistrationResult : IRegistrationResult
{
    public RegistrationResult(IReadOnlyList<Type> registeredTypes, TimeSpan elapsedTime)
    {
        RegisteredTypes = registeredTypes;
        RegisteredTypesCount = registeredTypes.Count;
        ElapsedTime = elapsedTime;
    }

    public TimeSpan ElapsedTime { get; }

    public IReadOnlyList<Type> RegisteredTypes { get; }

    public int RegisteredTypesCount { get; }
}
