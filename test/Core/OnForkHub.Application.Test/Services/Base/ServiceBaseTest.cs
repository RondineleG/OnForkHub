namespace OnForkHub.Application.Test.Services.Base;

public class ServiceBaseTest
{
    private readonly IValidationService _validationService;
    private readonly TestService _serviceBase;

    public ServiceBaseTest()
    {
        _validationService = Substitute.For<IValidationService>();
        _serviceBase = new TestService(_validationService);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should execute operation successfully")]
    public async Task ShouldExecuteOperationSuccessfully()
    {
        var result = await _serviceBase.ExecuteOperationAsync(() => Task.FromResult(RequestResult<int>.Success(1)));

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle general exception")]
    public async Task ShouldHandleGeneralException()
    {
        var exception = new Exception("General error");

        var result = await _serviceBase.ExecuteOperationAsync<int>(() => throw exception);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Be("Error processing operation: General error");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should execute operation with valid entity successfully")]
    public async Task ShouldExecuteOperationWithValidEntitySuccessfully()
    {
        var entity = new TestEntity { Name = "Test" };

        var result = await _serviceBase.ExecuteOperationAsync(entity, e => Task.FromResult(RequestResult<TestEntity>.Success(e)), ValidateTestEntity);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(entity);
    }

    private static CustomValidationResult ValidateTestEntity(TestEntity entity)
    {
        var validationResult = new CustomValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(entity.Name, "Name is required", nameof(entity.Name));
        return validationResult;
    }

    private class TestService(IValidationService validationService) : BaseService
    {
        public async Task<RequestResult<T>> ExecuteOperationAsync<T>(Func<Task<RequestResult<T>>> operation)
        {
            return await ExecuteAsync(operation);
        }

        public async Task<RequestResult<T>> ExecuteOperationAsync<T>(
            T entity,
            Func<T, Task<RequestResult<T>>> operation,
            Func<T, CustomValidationResult> validationFunc
        )
            where T : class
        {
            return await ExecuteAsync(entity, operation, validationFunc);
        }
    }

    private class TestEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
