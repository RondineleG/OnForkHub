namespace OnForkHub.Core.ValueObjects;

public class Name : ValueObject
{
    private const int MinNameLength = 3;
    private const int MaxNameLength = 50;

    private Name(string value)
    {
        Value = value;
        Validate();
    }

    public string Value { get; }

    public static Name Create(string value)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(value), NameResources.NameEmpty);

        return new Name(value);
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();

        validationResult
            .AddErrorIf(() => string.IsNullOrWhiteSpace(Value), NameResources.NameIsRequired, nameof(Name))
            .AddErrorIf(() => Value.Length < MinNameLength, NameResources.NameMinLength, nameof(Name))
            .AddErrorIf(() => Value.Length > MaxNameLength, NameResources.NameMaxLength, nameof(Name));
        return validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower(CultureInfo.CurrentCulture);
    }
}
