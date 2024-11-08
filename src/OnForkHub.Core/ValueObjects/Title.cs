namespace OnForkHub.Core.ValueObjects;

public class Title : ValueObject
{
    public string Value { get; set; }

    private Title(string value)
    {
        Value = value;
        Validate();
    }

    public static Title Create(string value)
    {
        var title = new Title(value);
        return title;
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Value, TitleResources.TitleRequired, nameof(Title));
        validationResult.AddErrorIf(Value.Length < 3, TitleResources.TitleMinLength, nameof(Title));
        validationResult.AddErrorIf(Value.Length > 50, TitleResources.TitleMaxLength, nameof(Title));
        return validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value.ToLower(CultureInfo.CurrentCulture);
    }
}
