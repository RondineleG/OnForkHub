namespace OnForkHub.TestExtensions.Assertions;

public static class ValidationResultAssertions
{
    public static void HaveValidationError(this ValidationResult result, string field, string expectedMessage)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Field.Should().Be(field);
        result.ErrorMessage.Should().Be($"{field}: {expectedMessage}");
    }

    public static void HaveMultipleErrors(this ValidationResult result, params (string field, string message)[] expectedErrors)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(expectedErrors.Length);

        foreach (var (field, message) in expectedErrors)
        {
            result.Errors.Should().Contain(e => e.Field == field && e.Message == message);
        }
    }

    public static void BeValid(this ValidationResult result)
    {
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
        result.ErrorMessage.Should().BeNullOrEmpty();
    }

    public static void HaveErrorsInOrder(this ValidationResult result, params (string field, string message)[] expectedErrors)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(expectedErrors.Length);

        for (var i = 0; i < expectedErrors.Length; i++)
        {
            result.Errors.ElementAt(i).Field.Should().Be(expectedErrors[i].field);
            result.Errors.ElementAt(i).Message.Should().Be(expectedErrors[i].message);
        }
    }

    public static void NotHaveErrorForField(this ValidationResult result, string field)
    {
        result.Errors.Should().NotContain(e => e.Field == field);
    }

    public static void HaveAtLeastOneError(this ValidationResult result)
    {
        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    public static void HaveAllErrorsContain(this ValidationResult result, string messagePattern)
    {
        result.Errors.Should().OnlyContain(e => e.Message.Contains(messagePattern));
    }

    public static void HaveDistinctErrorMessages(this ValidationResult result)
    {
        result.Errors.Select(e => e.Message).Should().OnlyHaveUniqueItems();
    }
}