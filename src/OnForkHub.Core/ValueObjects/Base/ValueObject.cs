namespace OnForkHub.Core.ValueObjects.Base;

public abstract class ValueObject
{
    public static bool EqualOperator(ValueObject left, ValueObject right)
    {
        return !(left is null ^ right is null) && (left is null || left.Equals(right));
    }

    public static bool NotEqualOperator(ValueObject left, ValueObject right)
    {
        return !EqualOperator(left, right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null || obj.GetType() != GetType())
        {
            return false;
        }
        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents().Select(x => x != null ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);
    }

    public abstract CustomValidationResult Validate();
    protected abstract IEnumerable<object> GetEqualityComponents();
}
