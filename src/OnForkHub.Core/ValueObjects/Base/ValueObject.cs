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
        return GetEqualityComponents().Select(x => (x != null) ? x.GetHashCode() : 0).Aggregate((x, y) => x ^ y);
    }

    public static bool EqualOperator(ValueObject left, ValueObject right)
    {
        return !(left is null ^ right is null) && (left is null || left.Equals(right));
    }

    protected abstract IEnumerable<object> GetEqualityComponents();
}
