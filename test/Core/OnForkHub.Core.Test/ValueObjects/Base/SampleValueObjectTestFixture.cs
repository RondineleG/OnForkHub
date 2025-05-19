// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Test.ValueObjects.Base;

public class SampleValueObjectTestFixture(int property1, string property2) : ValueObject
{
    public int Property1 { get; } = property1;

    public string Property2 { get; } = property2;

    public override ValidationResult Validate()
    {
        throw new NotImplementedException();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Property1;
        yield return Property2;
    }
}
