namespace OnForkHub.CrossCutting.Test.Logging;

using FluentAssertions;
using OnForkHub.CrossCutting.Logging;
using OnForkHub.CrossCutting.Logging.Implementations;
using Xunit;

[Trait("Category", "Unit")]
public class InMemoryErrorLoggerTests
{
    private readonly InMemoryErrorLogger _errorLogger = new();

    [Fact]
    [Trait("Method", "LogExceptionAsync")]
    public async Task LogExceptionAsyncShouldLogExceptionAndReturnErrorId()
    {
        var exception = new ArgumentNullException(nameof(LogExceptionAsyncShouldLogExceptionAndReturnErrorId));
        const string context = "Test context";

        var errorId = await _errorLogger.LogExceptionAsync(exception, context);

        errorId.Should().NotBeNullOrEmpty();
        var logEntry = await _errorLogger.GetErrorLogAsync(errorId);
        logEntry.Should().NotBeNull();
        logEntry!.Message.Should().Be(exception.Message);
        logEntry.Context.Should().Be(context);
        logEntry.ExceptionType.Should().Be(nameof(ArgumentNullException));
    }

    [Fact]
    [Trait("Method", "LogExceptionAsync")]
    public async Task LogExceptionAsyncShouldStoreCorrelationId()
    {
        var exception = new InvalidOperationException("Test exception");
        const string correlationId = "test-correlation-123";

        var errorId = await _errorLogger.LogExceptionAsync(exception, correlationId: correlationId);

        var logEntry = await _errorLogger.GetErrorLogAsync(errorId);
        logEntry!.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    [Trait("Method", "LogValidationErrorAsync")]
    public async Task LogValidationErrorAsyncShouldLogValidationErrors()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Email",
                new List<string> { "Invalid email format" }
            },
            {
                "Password",
                new List<string> { "Too weak", "Must contain numbers" }
            },
        };

        var errorId = await _errorLogger.LogValidationErrorAsync(errors);

        errorId.Should().NotBeNullOrEmpty();
        var logEntry = await _errorLogger.GetErrorLogAsync(errorId);
        logEntry.Should().NotBeNull();
        logEntry!.ValidationErrors.Should().HaveCount(2);
        logEntry.ValidationErrors!["Password"].Should().HaveCount(2);
    }

    [Fact]
    [Trait("Method", "LogValidationErrorAsync")]
    public async Task LogValidationErrorAsyncShouldStoreUserIdAndCorrelationId()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };
        const string userId = "user-123";
        const string correlationId = "corr-456";

        var errorId = await _errorLogger.LogValidationErrorAsync(errors, userId, correlationId);

        var logEntry = await _errorLogger.GetErrorLogAsync(errorId);
        logEntry!.UserId.Should().Be(userId);
        logEntry.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    [Trait("Method", "LogBusinessErrorAsync")]
    public async Task LogBusinessErrorAsyncShouldLogBusinessError()
    {
        const string errorMessage = "Order cannot be processed";
        const string errorCode = "BUSINESS_ERROR_001";

        var errorId = await _errorLogger.LogBusinessErrorAsync(errorMessage, errorCode);

        errorId.Should().NotBeNullOrEmpty();
        var logEntry = await _errorLogger.GetErrorLogAsync(errorId);
        logEntry.Should().NotBeNull();
        logEntry!.Message.Should().Be(errorMessage);
        logEntry.ErrorCode.Should().Be(errorCode);
        logEntry.ExceptionType.Should().Be("BusinessException");
    }

    [Fact]
    [Trait("Method", "LogBusinessErrorAsync")]
    public async Task LogBusinessErrorAsyncShouldSerializeAdditionalData()
    {
        const string errorMessage = "Test error";
        const string errorCode = "ERROR_CODE";
        var additionalData = new { OrderId = 123, Status = "Failed" };

        var errorId = await _errorLogger.LogBusinessErrorAsync(errorMessage, errorCode, additionalData);

        var logEntry = await _errorLogger.GetErrorLogAsync(errorId);
        logEntry!.AdditionalData.Should().NotBeNullOrEmpty();
        logEntry.AdditionalData.Should().Contain("OrderId");
    }

    [Fact]
    [Trait("Method", "GetErrorLogAsync")]
    public async Task GetErrorLogAsyncShouldReturnNullForNonexistentId()
    {
        var logEntry = await _errorLogger.GetErrorLogAsync("nonexistent-id");

        logEntry.Should().BeNull();
    }

    [Fact]
    [Trait("Method", "GetErrorLogsAsync")]
    public async Task GetErrorLogsAsyncShouldFilterByCorrelationId()
    {
        const string correlationId = "test-corr-id";
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };

        await _errorLogger.LogValidationErrorAsync(errors, correlationId: correlationId);
        await _errorLogger.LogValidationErrorAsync(errors, correlationId: "other-id");

        var logs = await _errorLogger.GetErrorLogsAsync(correlationId: correlationId);

        logs.Should().HaveCount(1);
        logs.First().CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    [Trait("Method", "GetErrorLogsAsync")]
    public async Task GetErrorLogsAsyncShouldFilterByUserId()
    {
        const string userId = "user-456";
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };

        await _errorLogger.LogValidationErrorAsync(errors, userId);
        await _errorLogger.LogValidationErrorAsync(errors, "other-user");

        var logs = await _errorLogger.GetErrorLogsAsync(userId: userId);

        logs.Should().HaveCount(1);
        logs.First().UserId.Should().Be(userId);
    }

    [Fact]
    [Trait("Method", "GetErrorLogsAsync")]
    public async Task GetErrorLogsAsyncShouldFilterByDateRange()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };

        var beforeDate = DateTime.UtcNow.AddHours(-1);
        await _errorLogger.LogValidationErrorAsync(errors);
        var afterDate = DateTime.UtcNow.AddHours(1);

        var logs = await _errorLogger.GetErrorLogsAsync(fromDate: beforeDate, toDate: afterDate);

        logs.Should().HaveCount(1);
    }

    [Fact]
    [Trait("Method", "GetErrorLogsAsync")]
    public async Task GetErrorLogsAsyncShouldRespectTakeParameter()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };

        for (int i = 0; i < 10; i++)
        {
            await _errorLogger.LogValidationErrorAsync(errors);
        }

        var logs = await _errorLogger.GetErrorLogsAsync(take: 5);

        logs.Should().HaveCount(5);
    }

    [Fact]
    [Trait("Method", "GetErrorLogsAsync")]
    public async Task GetErrorLogsAsyncShouldReturnMostRecentFirst()
    {
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };

        var firstId = await _errorLogger.LogValidationErrorAsync(errors);
        await Task.Delay(100);
        var secondId = await _errorLogger.LogValidationErrorAsync(errors);

        var logs = await _errorLogger.GetErrorLogsAsync();

        logs.First().ErrorId.Should().Be(secondId);
        logs.Last().ErrorId.Should().Be(firstId);
    }

    [Fact]
    [Trait("Method", "GetErrorLogsAsync")]
    public async Task GetErrorLogsAsyncShouldApplyMultipleFilters()
    {
        const string userId = "user-789";
        const string correlationId = "corr-789";
        var errors = new Dictionary<string, List<string>>
        {
            {
                "Field",
                new List<string> { "Error" }
            },
        };

        await _errorLogger.LogValidationErrorAsync(errors, userId, correlationId);
        await _errorLogger.LogValidationErrorAsync(errors, "other-user", "other-corr");

        var logs = await _errorLogger.GetErrorLogsAsync(userId: userId, correlationId: correlationId);

        logs.Should().HaveCount(1);
        logs.First().UserId.Should().Be(userId);
        logs.First().CorrelationId.Should().Be(correlationId);
    }
}
