namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestError : IRequestResult
{
    RequestError? RequestError { get; }
}
