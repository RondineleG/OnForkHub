using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Interfaces.Requests;

public interface IRequestEntityWarning : IRequestResult
{
    RequestEntityWarning? RequestEntityWarning { get; }
}
