namespace OnForkHub.Core.ValueObjects;

public class Url : ValueObject
{
    private Url(string value)
    {
        Value = value;
        Validate();
    }

    public string Value { get; }

    public static Url Create(string url)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(url.Trim()), UrlResources.UrlRequired);
        var normalizedUrl = url.EndsWith('/') && url.Length > 1 ? url.TrimEnd('/') : url;
        var urlObj = new Url(normalizedUrl);
        urlObj.Validate();
        return urlObj;
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        DomainException.ThrowErrorWhen(() => !Uri.IsWellFormedUriString(Value, UriKind.Absolute), UrlResources.UrlInvalid);

        var uri = new Uri(Value, UriKind.Absolute);
        DomainException.ThrowErrorWhen(() => uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps, UrlResources.UrlInvalid);
        validationResult.ThrowIfInvalid();
        return validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower(CultureInfo.CurrentCulture);
    }
}
