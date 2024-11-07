namespace OnForkHub.Core.Test.Entities.Base;

public class BaseEntityTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set CreatedAt correctly when instantiating entity")]
    public void CreatedAtShouldNotBeDefaultWhenInstantiatingEntity()
    {
        var entity = new ValidEntityTestFixture();
        entity.CreatedAt.Should().NotBe(default);
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
        entity.UpdatedAt.Should().BeNull();
        entity.Id.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should maintain CreatedAt in UTC time zone")]
    public void ShouldMaintainCreatedAtInUtcTimeZone()
    {
        var creationDate = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var entity = new ValidEntityTestFixture(1, creationDate);

        entity.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entity.CreatedAt.Should().Be(creationDate);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should not change UpdatedAt without calling UpdateCategory method")]
    public void ShouldNotChangeUpdatedAtWithoutCallingUpdateMethod()
    {
        var entity = new ValidEntityTestFixture();
        entity.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set all properties when providing an update date")]
    public void ShouldSetAllPropertiesWhenProvidingUpdateDate()
    {
        var id = 1L;
        var creationDate = DateTime.UtcNow.AddDays(-1);
        var updateDate = DateTime.UtcNow;

        var entity = new ValidEntityTestFixture(id, creationDate, updateDate);

        entity.Id.Should().Be(id);
        entity.CreatedAt.Should().Be(creationDate);
        entity.UpdatedAt.Should().Be(updateDate);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for ID with maximum negative value")]
    public void ShouldThrowExceptionForIdWithMaxNegativeValue()
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(-long.MaxValue, creationDate);

        action.Should().Throw<DomainException>().WithMessage(BaseEntityResources.IdGreaterThanZero);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when entity is invalid on executing UpdateCategory")]
    public void ShouldThrowExceptionWhenEntityIsInvalidOnExecutingUpdate()
    {
        var entity = new InvalidEntityTestFixture();

        Action action = entity.ExecuteUpdate;

        action.Should().Throw<DomainException>().WithMessage(BaseEntityResources.IdGreaterThanZero);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for invalid ID")]
    public void ShouldThrowExceptionWhenIdIsInvalid(long id)
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(id, creationDate);

        action.Should().Throw<DomainException>().WithMessage(BaseEntityResources.IdGreaterThanZero);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception with specific message for invalid ID")]
    public void ShouldThrowExceptionWithSpecificMessageWhenIdIsInvalid(long id)
    {
        var creationDate = DateTime.UtcNow;

        Action action = () => new ValidEntityTestFixture(id, creationDate);

        action.Should().Throw<DomainException>().WithMessage(BaseEntityResources.IdGreaterThanZero);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update UpdatedAt to recent time when calling UpdateCategory multiple times")]
    public void ShouldUpdateUpdatedAtToRecentTimeAfterExecutingUpdateMultipleTimes()
    {
        var entity = new ValidEntityTestFixture();

        entity.ExecuteUpdate();
        var firstUpdate = entity.UpdatedAt;

        Thread.Sleep(100);

        entity.ExecuteUpdate();
        var secondUpdate = entity.UpdatedAt;

        firstUpdate.Should().NotBe(secondUpdate);
        secondUpdate.Should().BeAfter(firstUpdate.Value);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should update UpdatedAt when executing UpdateCategory method")]
    public void ShouldUpdateUpdatedAtWhenExecutingUpdate()
    {
        var entity = new ValidEntityTestFixture();
        var beforeUpdate = DateTime.UtcNow;

        entity.ExecuteUpdate();

        entity.UpdatedAt.Should().NotBeNull();
        entity.UpdatedAt.Should().BeCloseTo(beforeUpdate, TimeSpan.FromSeconds(1));
    }
}
