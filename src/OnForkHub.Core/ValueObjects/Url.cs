namespace OnForkHub.Core.ValueObjects;

public class Url : ValueObject
{
    private readonly ValidationResult _validationResult;

    public string Value { get; private set; }

    private Url(string value)
    {
        Value = value;
        Validate();
    }

    public static Url Create(string url)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(url.Trim()), $"{nameof(Url)} cannot be empty");

        var normalizedUrl = url.EndsWith("/") && url.Length > 1 ? url.TrimEnd('/') : url;
        var urlObj = new Url(normalizedUrl);
        urlObj.Validate();
        return urlObj;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower(CultureInfo.CurrentCulture);
    }

    public override ValidationResult Validate()
    {
        DomainException.ThrowErrorWhen(() => !Uri.IsWellFormedUriString(Value, UriKind.Absolute), "Invalid URL");

        var uri = new Uri(Value, UriKind.Absolute);
        DomainException.ThrowErrorWhen(
            () => uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps,
            "Invalid URL"
        );

        return _validationResult;
    }
}
