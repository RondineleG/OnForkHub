namespace OnForkHub.TestExtensions.Assertions;

public static class ValidationErrorAssertions
{
    public static void ContainErrorMessage(this IEnumerable<ValidationErrorMessage> errors, string expectedMessage)
    {
        errors.Should().Contain(e => e.Message == expectedMessage);
    }

    public static void HaveAllFieldsStartWith(this IEnumerable<ValidationErrorMessage> errors, string prefix)
    {
        errors.Should().OnlyContain(e => e.Field.StartsWith(prefix));
    }

    public static void HaveErrorCount(this IEnumerable<ValidationErrorMessage> errors, int expectedCount)
    {
        errors.Should().HaveCount(expectedCount);
    }

    public static void HaveErrorsMatchingPattern(this IEnumerable<ValidationErrorMessage> errors, string messagePattern)
    {
        errors.Should().OnlyContain(e => e.Message.Contains(messagePattern));
    }

    public static void HaveFieldError(this ValidationErrorMessage error, string expectedField, string expectedMessage)
    {
        error.Field.Should().Be(expectedField);
        error.Message.Should().Be(expectedMessage);
    }

    public static void HaveFieldsWithError(this IEnumerable<ValidationErrorMessage> errors, params string[] expectedFields)
    {
        var actualFields = errors.Select(e => e.Field);
        actualFields.Should().BeEquivalentTo(expectedFields);
    }

    public static void HaveUniqueFields(this IEnumerable<ValidationErrorMessage> errors)
    {
        errors.Select(e => e.Field).Should().OnlyHaveUniqueItems();
    }

    public static void NotHaveFieldError(this IEnumerable<ValidationErrorMessage> errors, string field)
    {
        errors.Should().NotContain(e => e.Field == field);
    }
}