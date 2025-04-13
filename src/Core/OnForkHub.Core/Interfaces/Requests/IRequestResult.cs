namespace OnForkHub.Core.Interfaces.Requests;

public interface IRequestResult
{
    EResultStatus Status { get; }
}