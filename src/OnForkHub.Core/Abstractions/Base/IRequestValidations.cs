namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestValidations : IRequestResult
{
    IEnumerable<RequestValidation> Validations { get; }
}
