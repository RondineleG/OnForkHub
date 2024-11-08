using OnForkHub.Core.Requests;

namespace OnForkHub.Core.Interfaces.Requests;

public interface IRequestValidations : IRequestResult
{
    IEnumerable<RequestValidation> Validations { get; }
}
