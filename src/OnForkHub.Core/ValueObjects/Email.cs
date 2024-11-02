namespace OnForkHub.Core.ValueObjects;

public class Email : ValueObject
{
    private Email(string value)
    {
        Value = value;
        Validate();
    }

    public string Value { get; private set; }

    public static Email Create(string value)
    {
        DomainException.ThrowErrorWhen(
            () => string.IsNullOrWhiteSpace(value),
            "Email não pode ser vazio"
        );
        var email = new Email(value);
        email.Validate();
        return email;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower();
    }

    private void Validate()
    {
        var regex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
        DomainException.ThrowErrorWhen(() => !regex.IsMatch(Value), "Email inválido");
    }
}
