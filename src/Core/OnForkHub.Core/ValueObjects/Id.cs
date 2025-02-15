namespace OnForkHub.Core.ValueObjects;

public sealed class Id : ValueObject
{
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
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(value), IdResources.IdEmpty);
        return Create(value);
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

    private static Id Create(string value)
    {
        DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(value), IdResources.IdEmpty);

        if (!Guid.TryParseExact(value, "N", out var guid))
        {
            throw new DomainException(IdResources.InvalidIdFormat);
        }

        DomainException.ThrowErrorWhen(() => guid == Guid.Empty, IdResources.IdEmpty);
        return new Id(guid);
    }
}