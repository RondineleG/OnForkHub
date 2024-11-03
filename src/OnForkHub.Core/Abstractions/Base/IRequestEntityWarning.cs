namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestEntityWarning : IRequestResult
{
    RequestEntityWarning? EntityWarning { get; }
}
