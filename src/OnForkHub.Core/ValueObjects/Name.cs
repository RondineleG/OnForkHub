using OnForkHub.Core.Validations;

namespace OnForkHub.Core.ValueObjects;

public class Name : ValueObject
{
    public string Value { get; set; }

    private Name(string value)
    {
        Value = value;
        Validate();
    }

    public static Name Create(string value)
    {
        var name = new Name(value);
        name.Validate();
        return name;
    }

    private ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Value, "Nome é obrigatório", "Nome");
        validationResult.AddErrorIf(Value.Length < 3, "Nome deve ter pelo menos 3 caracteres", "Nome");
        validationResult.AddErrorIf(Value.Length > 50, "Nome deve ter no máximo 50 caracteres", "Nome");
        validationResult.ThrowIfInvalid();
        return validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower();
    }
}
