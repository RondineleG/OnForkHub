namespace OnForkHub.Core.ValueObjects;

public class Email : ValueObject
{
    public string Value { get; private set; } = null!;

    private Email(string value)
    {
        Value = value;
        Validate();
    }

    public static Email Create(string value)
    {
        DomainException.When(string.IsNullOrWhiteSpace(value),
            "Email não pode ser vazio");

        var email = new Email(value);
        email.Validate();
        return email;
    }

    private void Validate()
    {
        var regex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        DomainException.When(!regex.IsMatch(Value), "Email inválido");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower();
    }
}

