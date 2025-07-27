namespace OnForkHub.Application.Test.Services.Base;

public class ServiceBaseTest
{
    public ServiceBaseTest()
    {
        _serviceBase = new TestService();
    }

    private readonly TestService _serviceBase;

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
    [DisplayName("Should execute operation with valid entity successfully")]
    public async Task ShouldExecuteOperationWithValidEntitySuccessfully()
    {
        var entity = new TestEntity { Name = "Test" };

        var result = await _serviceBase.ExecuteOperationAsync(entity, e => Task.FromResult(RequestResult<TestEntity>.Success(e)), ValidateTestEntity);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().Be(entity);
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

    private static ValidationResult ValidateTestEntity(TestEntity entity)
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIf(() => string.IsNullOrWhiteSpace(entity.Name), "Name is required", nameof(entity.Name));
        return validationResult;
    }

    public class TestService : BaseService
    {
        public Task<RequestResult<T>> ExecuteOperationAsync<T>(Func<Task<RequestResult<T>>> operation)
        {
            return ExecuteAsync(operation);
        }

        public Task<RequestResult<T>> ExecuteOperationAsync<T>(
            T entity,
            Func<T, Task<RequestResult<T>>> operation,
            Func<T, ValidationResult> validationFunc
        )
            where T : class
        {
            return ExecuteAsync(entity, operation, validationFunc);
        }
    }

    private sealed class TestEntity
    {
        public string Name { get; set; } = string.Empty;
    }
}
