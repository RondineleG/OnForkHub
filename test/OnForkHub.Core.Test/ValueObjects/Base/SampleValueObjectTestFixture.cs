using OnForkHub.Core.ValueObjects.Base;

namespace OnForkHub.Core.Test.ValueObjects.Base;

public class SampleValueObjectTestFixture(int property1, string property2) : ValueObject
{
    public int Property1 { get; } = property1;

    public string Property2 { get; } = property2;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Property1;
        yield return Property2;
    }
}
