namespace OnForkHub.Core.ValueObjects;

public partial class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
        Validate();
    }

    public string Value { get; private set; }

    public static Email Create(string value)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(value), $"{nameof(Email)} cannot be empty");
        var email = new Email(value);
        email.Validate();
        return email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower(System.Globalization.CultureInfo.CurrentCulture);
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex MyRegex();

    private void Validate()
    {
        var regex = MyRegex();
        DomainException.ThrowErrorWhen(() => !regex.IsMatch(Value), "Invalid email");
    }
}
