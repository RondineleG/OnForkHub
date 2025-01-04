namespace OnForkHub.Core.Test.ValueObjects;

public class IdTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create new Id with valid Guid")]
    public void ShouldCreateNewIdWithValidGuid()
    {
        var id = Id.Create();

        id.Should().NotBeNull();
        id.Value.Should().NotBe(Guid.Empty);
        var validationResult = id.Validate();
        validationResult.IsValid.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should convert valid string to Id using implicit operator")]
    public void ShouldConvertValidStringToIdUsingImplicitOperator()
    {
        var guidString = Guid.NewGuid().ToString("N");

        Id id = guidString;

        id.Should().NotBeNull();
        id.Value.Should().NotBe(Guid.Empty);
        id.ToString().Should().Be(guidString);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw DomainException when converting invalid string to Id")]
    public void ShouldThrowDomainExceptionWhenConvertingInvalidStringToId(string? value)
    {
        var action = () =>
        {
            Id id = value;
        };

        action.Should().Throw<DomainException>().WithMessage(IdResources.IdEmpty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw DomainException when converting invalid Guid format")]
    public void ShouldThrowDomainExceptionWhenConvertingInvalidGuidFormat()
    {
        var invalidGuid = "invalid-guid-format";

        var action = () =>
        {
            Id id = invalidGuid;
        };

        action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should convert Id to string in N format")]
    public void ShouldConvertIdToStringInNFormat()
    {
        var id = Id.Create();

        string result = id;

        result.Should().Be(id.Value.ToString("N"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should convert Id to Guid")]
    public void ShouldConvertIdToGuid()
    {
        var id = Id.Create();

        Guid result = id;

        result.Should().Be(id.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider equal Ids with same value")]
    public void ShouldConsiderEqualIdsWithSameValue()
    {
        var guidString = Guid.NewGuid().ToString("N");
        Id id1 = guidString;
        Id id2 = guidString;

        id1.Should().Be(id2);
        id1.GetHashCode().Should().Be(id2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider different Ids with different values")]
    public void ShouldConsiderDifferentIdsWithDifferentValues()
    {
        var id1 = Id.Create();
        var id2 = Id.Create();

        id1.Should().NotBe(id2);
        id1.GetHashCode().Should().NotBe(id2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should maintain value consistency across implicit conversions")]
    public void ShouldMaintainValueConsistencyAcrossImplicitConversions()
    {
        var id = Id.Create();

        string stringValue = id;
        Guid guidValue = id;
        Id reconvertedId = stringValue;

        stringValue.Should().Be(id.Value.ToString("N"));
        guidValue.Should().Be(id.Value);
        reconvertedId.Should().Be(id);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw DomainException when trying to convert empty Guid string")]
    public void ShouldThrowDomainExceptionWhenTryingToConvertEmptyGuidString()
    {
        var emptyGuidString = Guid.Empty.ToString("N");

        var action = () =>
        {
            Id id = emptyGuidString;
        };

        action.Should().Throw<DomainException>().WithMessage(IdResources.IdEmpty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate Id correctly")]
    public void ShouldValidateIdCorrectly()
    {
        var id = Id.Create();

        var validationResult = id.Validate();

        validationResult.IsValid.Should().BeTrue();
        validationResult.Errors.Should().BeEmpty();
    }
}