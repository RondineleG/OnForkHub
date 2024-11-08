namespace OnForkHub.Core.ValueObjects;

public class Url : ValueObject
{
    public string Value { get; private set; }

    private Url(string value)
    {
        Value = value;
        Validate();
    }

    public static Url Create(string url)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(url.Trim()), UrlResources.UrlRequired);
        var normalizedUrl = url.EndsWith('/') && url.Length > 1 ? url.TrimEnd('/') : url;
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
        var _validationResult = new ValidationResult();
        DomainException.ThrowErrorWhen(() => !Uri.IsWellFormedUriString(Value, UriKind.Absolute), UrlResources.UrlInvalid);

        var uri = new Uri(Value, UriKind.Absolute);
        DomainException.ThrowErrorWhen(() => uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps, UrlResources.UrlInvalid);
        _validationResult.ThrowIfInvalid();
        return _validationResult;
    }
}
