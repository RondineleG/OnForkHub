namespace OnForkHub.Core.Interfaces.Requests;

public interface IRequestError : IRequestResult
{
    RequestError? RequestError { get; }
}
