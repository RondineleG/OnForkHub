namespace OnForkHub.Core.Abstractions.Base;

public interface IRequestValidations : IRequestResult
{
    IEnumerable<Validation> Validations { get; }
}
