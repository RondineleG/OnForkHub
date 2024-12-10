namespace OnForkHub.Core.Test.ValueObjects.Base;

public class ValueObjectTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider both objects equal when both are null")]
    public void ShouldConsiderBothObjectsEqualWhenBothAreNull()
    {
        SampleValueObjectTestFixture? obj1 = null;
        SampleValueObjectTestFixture? obj2 = null;

        ValueObject.EqualOperator(obj1, obj2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider objects different when properties are different")]
    public void ShouldConsiderObjectsDifferentWhenPropertiesAreDifferent()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(2, "Different");

        obj1.Equals(obj2).Should().BeFalse();
        obj1.Should().NotBe(obj2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider objects equal when properties are equal")]
    public void ShouldConsiderObjectsEqualWhenPropertiesAreEqual()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(1, "Test");

        obj1.Equals(obj2).Should().BeTrue();
        obj1.Should().Be(obj2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider objects with different property values not equal using implicit equality")]
    public void ShouldConsiderObjectsWithDifferentPropertyValuesNotEqualUsingImplicitEquality()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(2, "Another");

        (obj1 != obj2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider objects with identical property values equal using implicit equality")]
    public void ShouldConsiderObjectsWithIdenticalPropertyValuesEqualUsingImplicitEquality()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(1, "Test");

        var areEqual = ValueObject.EqualOperator(obj1, obj2);
        areEqual.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return consistent hash codes on consecutive calls")]
    public void ShouldReturnConsistentHashCodesOnConsecutiveCalls()
    {
        var obj = new SampleValueObjectTestFixture(1, "Consistent");

        var hash1 = obj.GetHashCode();
        var hash2 = obj.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return different hash code for objects with different properties")]
    public void ShouldReturnDifferentHashCodeForObjectsWithDifferentProperties()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(2, "Different");

        obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when comparing a null object with a non-null object")]
    public void ShouldReturnFalseWhenComparingNullWithNonNull()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        SampleValueObjectTestFixture obj2 = null;

        ValueObject.EqualOperator(obj1, obj2).Should().BeFalse();
        ValueObject.EqualOperator(obj2, obj1).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when comparing with different types")]
    public void ShouldReturnFalseWhenComparingWithDifferentTypes()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var differentTypeObj = "I am not a ValueObject";

        obj1.Equals(differentTypeObj).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return false when comparing with null")]
    public void ShouldReturnFalseWhenComparingWithNull()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");

        obj1.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return same hash code for objects with equal properties")]
    public void ShouldReturnSameHashCodeForObjectsWithEqualProperties()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(1, "Test");

        obj1.GetHashCode().Should().Be(obj2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should support using equality comparison in collections")]
    public void ShouldSupportUsingEqualityComparisonInCollections()
    {
        var obj1 = new SampleValueObjectTestFixture(1, "Test");
        var obj2 = new SampleValueObjectTestFixture(1, "Test");
        var obj3 = new SampleValueObjectTestFixture(2, "Different");

        var set = new HashSet<SampleValueObjectTestFixture> { obj1, obj3 };

        set.Contains(obj2).Should().BeTrue();
        set.Contains(new SampleValueObjectTestFixture(2, "Different")).Should().BeTrue();
    }
}