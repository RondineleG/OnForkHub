namespace OnForkHub.Core.Test.Entities.Base;

public class BaseEntityTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set CreatedAt correctly when instantiating entity")]
    public void CreatedAtShouldNotBeDefaultWhenInstantiatingEntity()
    {
        var entity = new ValidEntityTestFixture();
        entity.CreatedAt.Should().NotBe(default);
        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(long.MaxValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Should create entity with valid ID")]
    public void ShouldCreateEntityWhenIdIsValid(long id)
    {
        var creationDate = DateTime.UtcNow;

        var entity = new ValidEntityTestFixture(id, creationDate);

        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(creationDate);
        entity.UpdatedAt.Should().BeNull();
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
        entity.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not throw when CreatedAt is UTC")]
    public void ShouldNotThrowWhenCreatedAtIsUtc()
    {
        var creationDate = DateTime.UtcNow;
        var fixture = new ValidEntityTestFixture(1, creationDate);
        Action action = () => fixture.ExecuteUpdate();
        action.Should().NotThrow();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when UpdatedAt is not UTC")]
    public void ShouldThrowWhenUpdatedAtIsNotUtc()
    {
        var creationDate = DateTime.UtcNow;
        var updateDate = DateTime.Now.AddDays(1).AddMilliseconds(1);

        Action action = () => new ValidEntityTestFixture(1, creationDate, updateDate);

        var exception = action.Should().Throw<DomainException>().Which;
        exception.Message.Should().Be("UpdatedAt must be UTC");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when UpdatedAt is not greater than CreatedAt")]
    public void ShouldThrowWhenUpdatedAtIsNotGreaterThanCreatedAt()
    {
        var creationDate = DateTime.UtcNow;
        var updateDate = creationDate.AddSeconds(-1);

        Action action = () => new ValidEntityTestFixture(1, creationDate, updateDate);

        action.Should().Throw<DomainException>().WithMessage("UpdatedAt must be greater than CreatedAt");
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

    [Theory]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for negative ID")]
    public void ShouldThrowExceptionForNegativeId(long id)
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(id, creationDate);

        action.Should().Throw<DomainException>().WithMessage("Id cannot be negative");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw for default CreatedAt with all validation errors")]
    public void ShouldThrowForDefaultCreatedAt()
    {
        static void action()
        {
            new ValidEntityTestFixture(1, default);
        }

        var exception = Assert.Throws<DomainException>(action);

        exception.Message.Split(';', StringSplitOptions.TrimEntries).Should().Contain(["CreatedAt is required", "CreatedAt must be UTC"]);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate state after multiple updates")]
    public void ShouldValidateStateAfterMultipleUpdates()
    {
        var entity = new ValidEntityTestFixture();

        entity.ExecuteUpdate();
        var firstUpdate = entity?.UpdatedAt;

        Thread.Sleep(100);

        entity.ExecuteUpdate();
        var secondUpdate = entity?.UpdatedAt;

        firstUpdate.Should().NotBe(secondUpdate);
        secondUpdate.Should().BeAfter(firstUpdate.Value);
        secondUpdate.Value.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate state when UpdatedAt is set to current time")]
    public void ShouldValidateStateWhenUpdatedAtIsSetToCurrentTime()
    {
        var entity = new ValidEntityTestFixture();
        var beforeUpdate = DateTime.UtcNow;

        entity.ExecuteUpdate();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeAfter(entity.CreatedAt);
        entity.UpdatedAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw when entity has invalid state")]
    public void ShouldThrowWhenEntityHasInvalidState()
    {
        var entity = new InvalidEntityTestFixture();

        Action action = entity.ForceValidation;

        action.Should().Throw<DomainException>().WithMessage("Invalid entity state");
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

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [Trait("Category", "Unit")]
    [DisplayName("Should create entity with valid non-negative ID")]
    public void ShouldCreateEntityWithValidNonNegativeId(long id)
    {
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
        var createdAt = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(1, createdAt, createdAt);

        action.Should().Throw<DomainException>().WithMessage("UpdatedAt must be greater than CreatedAt");
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

        updates.Should().HaveCount(numberOfUpdates);
        updates.Should().BeInAscendingOrder();
        updates
            .Should()
            .AllSatisfy(date =>
            {
                date.Should().NotBeNull();
                date!.Value.Kind.Should().Be(DateTimeKind.Utc);
                date.Should().BeAfter(entity.CreatedAt);
            });
    }
}
