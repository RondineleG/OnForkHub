using OnForkHub.Core.Enums;

namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestResult
{
    ECustomResultStatus Status { get; }
}
