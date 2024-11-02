namespace OnForkHub.Core.ValueObjects.Base;

public abstract class ValueObject
{
    public override bool Equals(object obj)
    {
        if ((obj == null) || (obj.GetType() != GetType()))
        {
            return false;
        }

        var other = (ValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => (x != null) ? x.GetHashCode() : 0)
            .Aggregate((x, y) => x ^ y);
    }

    protected static bool EqualOperator(ValueObject left, ValueObject right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
        {
            return false;
        }

        return ReferenceEquals(left, null) || left.Equals(right);
    }

    protected abstract IEnumerable<object> GetEqualityComponents();
}
