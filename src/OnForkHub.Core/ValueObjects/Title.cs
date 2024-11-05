using OnForkHub.Core.Validations;

namespace OnForkHub.Core.ValueObjects;

public class Title : ValueObject
{
    public string Value { get; set; }

    private Title(string value)
    {
        Value = value;
        Validate();
    }

    public static Title Create(string value)
    {
        var title = new Title(value);
        return title;
    }

    public ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Value, "Titulo é obrigatório", "Titulo");
        validationResult.AddErrorIf(Value.Length < 3, "Titulo deve ter pelo menos 3 caracteres", "Titulo");
        validationResult.AddErrorIf(Value.Length > 50, "Titulo deve ter no máximo 50 caracteres", "Titulo");
        validationResult.ThrowIfInvalid();
        return validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower();
    }
}
