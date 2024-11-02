namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestError : IRequestResult
{
    Error? Error { get; }
}
