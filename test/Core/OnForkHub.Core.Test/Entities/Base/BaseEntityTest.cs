// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Test.Entities.Base;

public class BaseEntityTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set CreatedAt correctly when instantiating entity")]
    public void CreatedAtShouldNotBeDefaultWhenInstantiatingEntity()
    {
        var entity = new ValidEntityTestFixture();
        var result = new ValidationResult();
        result.BeValid();
        entity.CreatedAt.Should().NotBe(default).And.BeIn(DateTimeKind.Utc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should include field name in error message")]
    public void ShouldIncludeFieldNameInErrorMessage()
    {
        var creationDate = DateTime.UtcNow;
        Action action = () => new ValidEntityTestFixture("invalid-guid", creationDate);

        action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should initialize properties when using default constructor")]
    public void ShouldInitializePropertiesWhenUsingDefaultConstructor()
    {
        var entity = new ValidEntityTestFixture();
        var currentDate = DateTime.UtcNow;

        entity.CreatedAt.Should().BeCloseTo(currentDate, TimeSpan.FromSeconds(1));
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entity.UpdatedAt.Should().BeNull();
        entity.Id.Should().NotBeNull();
        entity.Id.Should().NotBe(string.Empty);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should maintain UTC kind after multiple updates")]
    public void ShouldMaintainUtcKindAfterMultipleUpdates()
    {
        var entity = new ValidEntityTestFixture();

        entity.ExecuteUpdate();
        entity.ExecuteUpdate();
        entity.ExecuteUpdate();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData("invalid-guid-format")]
    [InlineData("invalid-guid")]
    [InlineData("categories/invalid-guid")]
    [InlineData("categories/123")]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for invalid ID format")]
    public void ShouldThrowExceptionForInvalidIdFormat(string invalidId)
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(invalidId, creationDate);

        action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw for default CreatedAt with all validation errors")]
    public void ShouldThrowForDefaultCreatedAt()
    {
        var id = Id.Create();
        Action action = () => new ValidEntityTestFixture(id, default);

        action
            .Should()
            .Throw<DomainException>()
            .WithValidationErrors((ValidationFields.CreatedAt, "CreatedAt is required"), (ValidationFields.CreatedAt, "CreatedAt must be UTC"));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when entity has invalid state")]
    public void ShouldThrowWhenEntityHasInvalidState()
    {
        var entity = new InvalidEntityTestFixture();
        var action = entity.ForceValidation;

        action.Should().Throw<DomainException>().WithMessage("Invalid entity state");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when UpdatedAt equals CreatedAt")]
    public void ShouldThrowWhenUpdatedAtEqualsCreatedAt()
    {
        var id = Id.Create();
        var createdAt = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(id, createdAt, createdAt);

        action.Should().Throw<DomainException>().WithValidationError(ValidationFields.UpdatedAt, "UpdatedAt must be greater than CreatedAt");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when UpdatedAt is not greater than CreatedAt")]
    public void ShouldThrowWhenUpdatedAtIsNotGreaterThanCreatedAt()
    {
        var id = Id.Create();
        var creationDate = DateTime.UtcNow;
        var updateDate = creationDate.AddSeconds(-1);

        Action action = () => new ValidEntityTestFixture(id, creationDate, updateDate);

        action.Should().Throw<DomainException>().WithValidationError(ValidationFields.UpdatedAt, "UpdatedAt must be greater than CreatedAt");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when UpdatedAt is not UTC")]
    public void ShouldThrowWhenUpdatedAtIsNotUtc()
    {
        var id = Id.Create();
        var creationDate = DateTime.UtcNow;
        var updateDate = DateTime.Now.AddDays(1).AddMilliseconds(1);

        Action action = () => new ValidEntityTestFixture(id, creationDate, updateDate);

        action.Should().Throw<DomainException>().WithValidationError(ValidationFields.UpdatedAt, "UpdatedAt must be UTC");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update UpdatedAt when executing Update method")]
    public void ShouldUpdateUpdatedAtWhenExecutingUpdate()
    {
        var entity = new ValidEntityTestFixture();
        var beforeUpdate = DateTime.UtcNow;

        entity.ExecuteUpdate();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt!.Value.Kind.Should().Be(DateTimeKind.Utc);
        entity.UpdatedAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate multiple sequential updates")]
    public void ShouldValidateMultipleSequentialUpdates(int numberOfUpdates)
    {
        var entity = new ValidEntityTestFixture();
        var updates = new List<DateTime?>();

        for (var i = 0; i < numberOfUpdates; i++)
        {
            entity.ExecuteUpdate();
            updates.Add(entity.UpdatedAt);
        }

        var result = new ValidationResult();
        result.BeValid();

        updates
            .Should()
            .HaveCount(numberOfUpdates)
            .And.BeInAscendingOrder()
            .And.AllSatisfy(date =>
            {
                date.Should().NotBeNull();
                date!.Value.Kind.Should().Be(DateTimeKind.Utc);
                date.Should().BeAfter(entity.CreatedAt);
            });
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate state after multiple updates")]
    public void ShouldValidateStateAfterMultipleUpdates()
    {
        var entity = new ValidEntityTestFixture();

        entity.ExecuteUpdate();
        var firstUpdate = entity.UpdatedAt;

        entity.ExecuteUpdate();
        var secondUpdate = entity.UpdatedAt;

        firstUpdate.Should().NotBeNull();
        secondUpdate.Should().NotBeNull();
        secondUpdate!.Value.Should().BeAfter(firstUpdate!.Value);
        secondUpdate.Value.Kind.Should().Be(DateTimeKind.Utc);
    }
}
