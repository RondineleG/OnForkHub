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

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Value, $"{nameof(Title)} is required", nameof(Title));
        validationResult.AddErrorIf(
            Value.Length < 3,
            $"{nameof(Title)} must be at least 3 characters long",
            nameof(Title)
        );
        validationResult.AddErrorIf(
            Value.Length > 50,
            $"{nameof(Title)} must be no more than 50 characters",
            nameof(Title)
        );
        return validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower();
    }
}
