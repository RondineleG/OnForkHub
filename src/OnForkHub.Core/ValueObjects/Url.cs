using OnForkHub.Core.Exceptions;
using OnForkHub.Core.ValueObjects.Base;

namespace OnForkHub.Core.ValueObjects;

public class Url : ValueObject
{
    public string Valor { get; private set; }

    private Url() { }

    public static Url Create(string url)
    {
        DomainException.When(string.IsNullOrWhiteSpace(url),
            "URL não pode ser vazia");

        var urlObj = new Url { Valor = url };
        urlObj.Validate();
        return urlObj;
    }

    private void Validate()
    {
        DomainException.When(!Uri.IsWellFormedUriString(Valor, UriKind.Absolute),
            "URL inválida");
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Valor.ToLower();
    }
}

