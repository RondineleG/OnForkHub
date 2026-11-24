namespace OnForkHub.Core.ValueObjects;

public sealed class Id : ValueObject
{
    // Parameterless constructor for EF Core
    private Id()
    {
        Value = Guid.Empty;
        _validationResult = new ValidationResult();
    }

    private Id(Guid value)
    {
        Value = value;
        _validationResult = new ValidationResult();
        Validate();
    }

    private readonly ValidationResult _validationResult;

    public Guid Value { get; }

    public static Id Create()
    {
        return new Id(Guid.NewGuid());
    }

    public static implicit operator Guid(Id id)
    {
        return id.Value;
    }

    public static implicit operator Id(string value)
    {
        // Tolerates legacy prefixed identifiers (e.g. "Video/{guid}") by using the segment after the last '/'.
        var candidate = value.Contains('/') ? value[(value.LastIndexOf('/') + 1)..] : value;

        return Guid.TryParse(candidate, out var guid) ? new Id(guid) : throw new DomainException(IdResources.InvalidIdFormat);
    }

    public static implicit operator string(Id id)
    {
        return id.ToString();
    }

    public override string ToString()
    {
        return Value.ToString("N");
    }

    public override ValidationResult Validate()
    {
        _validationResult.AddErrorIf(() => Value == Guid.Empty, IdResources.IdIsRequired, nameof(Id));
        return _validationResult;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
