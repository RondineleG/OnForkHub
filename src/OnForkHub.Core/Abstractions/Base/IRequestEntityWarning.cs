namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestEntityWarning : IRequestResult
{
    EntityWarning? EntityWarning { get; }
}