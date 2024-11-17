namespace OnForkHub.Core.Interfaces.Validations;

public interface IValidationBuilder
{
    IValidationBuilder WithField(string fieldName, object? value = null);
    IValidationBuilder NotNull(string? message = null);
    IValidationBuilder NotEmpty(string? message = null);
    IValidationBuilder NotWhiteSpace(string? message = null);
    IValidationBuilder MinLength(int length, string? message = null);
    IValidationBuilder MaxLength(int length, string? message = null);
    IValidationBuilder Length(int exactLength, string? message = null);
    IValidationBuilder Range<T>(T min, T max, string? message = null)
        where T : IComparable<T>;
    IValidationBuilder Matches(string pattern, string? message = null);
    IValidationBuilder Custom(Func<object?, bool> validation, string message);
    Task<IValidationBuilder> CustomAsync(Func<object?, Task<bool>> validation, string message);
    IValidationBuilder WithMetadata<T>(string key, T value);
    IValidationBuilder Ensure(Func<bool> predicate, string message);
    Task<IValidationBuilder> EnsureAsync(Func<Task<bool>> predicate, string message);
    IValidationResult Validate();
}
