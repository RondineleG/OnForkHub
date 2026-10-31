namespace OnForkHub.CrossCutting.DependencyInjection;

public interface IRegistrationResult
{
    TimeSpan ElapsedTime { get; }

    IReadOnlyList<Type> RegisteredTypes { get; }

    int RegisteredTypesCount { get; }
}
