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
    [DisplayName("Should create entity with valid ID")]
    public void ShouldCreateEntityWhenIdIsValid()
    {
        var id = Id.Create();
        var creationDate = DateTime.UtcNow;
        var entity = new ValidEntityTestFixture(id, creationDate);

        var result = new ValidationResult();
        result.BeValid();

        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(creationDate);
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for invalid ID format")]
    public void ShouldThrowExceptionForInvalidIdFormat()
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture("invalid-guid-format", creationDate);

        action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for empty ID")]
    public void ShouldThrowExceptionForEmptyId()
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(string.Empty, creationDate);

        action.Should().Throw<DomainException>().WithMessage(IdResources.IdEmpty);
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
        entity.Id.Value.Should().NotBe(Guid.Empty);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-guid")]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for invalid ID")]
    public void ShouldThrowExceptionForInvalidId(string invalidId)
    {
        var creationDate = DateTime.UtcNow;
        Action action = () => new ValidEntityTestFixture(invalidId, creationDate);

        if (string.IsNullOrWhiteSpace(invalidId))
        {
            action.Should().Throw<DomainException>().WithMessage(IdResources.IdEmpty);
        }
        else
        {
            action.Should().Throw<DomainException>().WithMessage(IdResources.InvalidIdFormat);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for empty GUID")]
    public void ShouldThrowExceptionForEmptyGuid()
    {
        var creationDate = DateTime.UtcNow;
        var emptyGuid = "00000000000000000000000000000000";

        Action action = () => new ValidEntityTestFixture(emptyGuid, creationDate);

        action.Should().Throw<DomainException>().WithMessage(IdResources.IdEmpty);
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
    [DisplayName("Should accept valid GUID as ID")]
    public void ShouldAcceptValidGuidAsId()
    {
        var creationDate = DateTime.UtcNow;
        var validId = Id.Create();

        var entity = new ValidEntityTestFixture(validId, creationDate);

        entity.Id.Should().Be(validId);
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
    [DisplayName("Should throw when UpdatedAt is not greater than CreatedAt")]
    public void ShouldThrowWhenUpdatedAtIsNotGreaterThanCreatedAt()
    {
        var id = Id.Create();
        var creationDate = DateTime.UtcNow;
        var updateDate = creationDate.AddSeconds(-1);

        Action action = () => new ValidEntityTestFixture(id, creationDate, updateDate);

        action.Should().Throw<DomainException>().WithValidationError(ValidationFields.UpdatedAt, "UpdatedAt must be greater than CreatedAt");
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
            Thread.Sleep(100);
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
    [DisplayName("Should throw when entity has invalid state")]
    public void ShouldThrowWhenEntityHasInvalidState()
    {
        var entity = new InvalidEntityTestFixture();
        var action = entity.ForceValidation;

        action.Should().Throw<DomainException>().WithMessage("Invalid entity state");
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
        entity.UpdatedAt.Value.Kind.Should().Be(DateTimeKind.Utc);
        entity.UpdatedAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate state after multiple updates")]
    public void ShouldValidateStateAfterMultipleUpdates()
    {
        var entity = new ValidEntityTestFixture();

        entity.ExecuteUpdate();
        var firstUpdate = entity.UpdatedAt;

        Thread.Sleep(100);

        entity.ExecuteUpdate();
        var secondUpdate = entity.UpdatedAt;

        firstUpdate.Should().NotBe(secondUpdate);
        secondUpdate.Should().BeAfter(firstUpdate.Value);
        secondUpdate.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should maintain UTC kind after multiple updates")]
    public void ShouldMaintainUtcKindAfterMultipleUpdates()
    {
        var entity = new ValidEntityTestFixture();

        entity.ExecuteUpdate();
        Thread.Sleep(100);
        entity.ExecuteUpdate();
        Thread.Sleep(100);
        entity.ExecuteUpdate();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Value.Kind.Should().Be(DateTimeKind.Utc);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create entity with valid Id value object")]
    public void ShouldCreateEntityWithValidIdValueObject()
    {
        var id = Id.Create();
        var createdAt = DateTime.UtcNow;
        var entity = new ValidEntityTestFixture(id, createdAt);

        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(createdAt);
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
}
