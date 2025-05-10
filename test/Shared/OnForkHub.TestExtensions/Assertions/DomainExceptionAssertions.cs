namespace OnForkHub.TestExtensions.Assertions;

public static class DomainExceptionAssertions
{
    public static void WithAllValidationFields(this ExceptionAssertions<DomainException> assertion, params string[] expectedFields)
    {
        var exception = assertion.Which;
        var actualFields = exception
            .Message.Split(';', StringSplitOptions.TrimEntries)
            .Select(error => error.Split(':', StringSplitOptions.TrimEntries)[0])
            .ToList();

        actualFields.Should().BeEquivalentTo(expectedFields);
    }

    public static void WithErrorCode(this ExceptionAssertions<DomainException> assertion, string expectedErrorCode)
    {
        assertion.Which.ErrorCode.Should().Be(expectedErrorCode);
    }

    public static void WithErrorContaining(this ExceptionAssertions<DomainException> assertion, string expectedText)
    {
        assertion.Which.Message.Should().Contain(expectedText);
    }

    public static void WithErrorCountGreaterThan(this ExceptionAssertions<DomainException> assertion, int count)
    {
        var errorCount = assertion.Which.Message.Split(';', StringSplitOptions.TrimEntries).Length;
        errorCount.Should().BeGreaterThan(count);
    }

    public static void WithErrorMessage(this ExceptionAssertions<DomainException> assertion, string expectedMessage)
    {
        assertion.Which.Message.Split(':', StringSplitOptions.TrimEntries)[1].Should().Be(expectedMessage);
    }

    public static void WithoutFieldErrors(this ExceptionAssertions<DomainException> assertion)
    {
        assertion.Which.Message.Should().NotContain(":");
    }

    public static void WithTimestamp(this ExceptionAssertions<DomainException> assertion, DateTimeOffset expectedTimestamp)
    {
        var exception = assertion.Which;
        var exceptionTimestamp =
            exception.Data["Timestamp"]?.ToString()
            ?? throw new KeyNotFoundException("The 'Timestamp' key was not found in the exception's Data dictionary.");

        DateTimeOffset
            .TryParse(exceptionTimestamp, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedTimestamp)
            .Should()
            .BeTrue($"The 'Timestamp' value in exception data is not a valid DateTimeOffset: {exceptionTimestamp}");

        parsedTimestamp.Should().Be(expectedTimestamp, "the exception timestamp should match the expected timestamp.");
    }

    public static void WithValidationError(this ExceptionAssertions<DomainException> assertion, string field, string expectedMessage)
    {
        var exception = assertion.Which;
        var fullMessage = $"{field}: {expectedMessage}";
        exception.Message.Should().Be(fullMessage);
    }

    public static void WithValidationErrorForField(this ExceptionAssertions<DomainException> assertion, string field)
    {
        var exception = assertion.Which;
        var errors = exception.Message.Split(';', StringSplitOptions.TrimEntries);

        errors.Should().Contain(e => e.StartsWith($"{field}:"));
    }

    public static void WithValidationErrorInFields(this ExceptionAssertions<DomainException> assertion, string[] fields, string expectedMessage)
    {
        var exception = assertion.Which;
        var errors = exception.Message.Split(';', StringSplitOptions.TrimEntries);

        fields.Length.Should().Be(errors.Length, "number of fields should match number of errors");

        foreach (var field in fields)
        {
            errors.Should().Contain($"{field}: {expectedMessage}");
        }
    }

    public static void WithValidationErrors(
        this ExceptionAssertions<DomainException> assertion,
        params (string field, string message)[] expectedErrors
    )
    {
        var exception = assertion.Which;
        var actualErrors = exception
            .Message.Split(';', StringSplitOptions.TrimEntries)
            .Select(error =>
            {
                var parts = error.Split(':', StringSplitOptions.TrimEntries);
                return (field: parts[0], message: parts[1]);
            })
            .ToList();

        actualErrors.Should().BeEquivalentTo(expectedErrors);
    }
}