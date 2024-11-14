namespace OnForkHub.Application.Test.Validation;

public class ValidationServiceFactoryTests
{
    private readonly ValidationServiceFactory _factory;

    public ValidationServiceFactoryTests()
    {
        _factory = new ValidationServiceFactory();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create category validation service")]
    public void ShouldCreateCategoryValidationService()
    {
        var service = _factory.CreateCategoryValidationService();

        service.Should().NotBeNull();
        service.Should().BeAssignableTo<ICategoryValidationService>();
    }
}
