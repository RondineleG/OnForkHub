namespace OnForkHub.Core.ValueObjects;

public partial class Email : ValueObject
{
    private Email(string value)
    {
        this.Value = value;
        this.Validate();
    }

    public string Value { get; private set; }

    public static Email Create(string value)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(value), "Email não pode ser vazio");
        var email = new Email(value);
        email.Validate();
        return email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return this.Value.ToLower(System.Globalization.CultureInfo.CurrentCulture);
    }

    private void Validate()
    {
        var regex = MyRegex();
        DomainException.ThrowErrorWhen(() => !regex.IsMatch(this.Value), "Email inválido");
    }

    [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
    private static partial Regex MyRegex();
}
