// The .NET Foundation licenses this file to you under the MIT license.

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
        if (value.Contains('/'))
        {
            var parts = value.Split('/');

            var idPart = parts[1].Split('-')[0];
            if (Guid.TryParse(idPart, out var guid))
            {
                return new Id(guid);
            }
        }

        return Guid.TryParse(value, out var normalGuid) ? new Id(normalGuid) : throw new DomainException(IdResources.InvalidIdFormat);
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
