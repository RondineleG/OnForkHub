namespace OnForkHub.Core.ValueObjects;

public class Url : ValueObject
{
    private Url() { }

    public string Valor { get; private set; }

    public static Url Create(string url)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(url), "URL não pode ser vazia");

        var urlObj = new Url { Valor = url };
        urlObj.Validate();
        return urlObj;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return this.Valor.ToLower(System.Globalization.CultureInfo.CurrentCulture);
    }

    private void Validate()
    {
        DomainException.ThrowErrorWhen(() => !Uri.IsWellFormedUriString(this.Valor, UriKind.Absolute), "URL inválida");
    }
}
