namespace OnForkHub.Core.Extensions;

public static class ValidationResultExtensions
{
    public static IValidationResult AddErrorIfNull<T>(this IValidationResult result, T? value, string message, string field = "")
        where T : class
    {
        return result.AddErrorIf(() => value is null, message, field);
    }

    public static IValidationResult AddErrorIfNullOrEmpty(this IValidationResult result, string? value, string message, string field = "")
    {
        return result.AddErrorIf(() => string.IsNullOrEmpty(value), message, field);
    }

    public static IValidationResult AddErrorIfNullOrWhiteSpace(this IValidationResult result, string? value, string message, string field = "")
    {
        return result.AddErrorIf(() => string.IsNullOrWhiteSpace(value), message, field);
    }
}