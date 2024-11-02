namespace OnForkHub.Core.ValueObjects;

public class Url : ValueObject
{
    private Url() { }

    public string Valor { get; private set; }

    public static Url Create(string url)
    {
        DomainException.ThrowErrorWhen(
            () => string.IsNullOrWhiteSpace(url),
            "URL não pode ser vazia"
        );

        var urlObj = new Url { Valor = url };
        urlObj.Validate();
        return urlObj;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor.ToLower();
    }

    private void Validate()
    {
        DomainException.ThrowErrorWhen(
            () => !Uri.IsWellFormedUriString(Valor, UriKind.Absolute),
            "URL inválida"
        );
    }
}
