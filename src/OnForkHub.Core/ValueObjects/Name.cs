using OnForkHub.Core.Entities;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects.Base;

using System.Globalization;

namespace OnForkHub.Core.ValueObjects
{
    public class Name : ValueObject
    {
        private const int MinNameLength = 3;
        private const int MaxNameLength = 50;

        public string Value { get; }

        private Name(string value)
        {
            Value = value;
            Validate();
        }

        public static Name Create(string value)
        {
            DomainException.ThrowErrorWhen(() => string.IsNullOrWhiteSpace(value),
                $"{nameof(Name)} cannot be empty or null");

            return new Name(value);
        }

        public override ValidationResult Validate()
        {
            var validationResult = new ValidationResult();

            validationResult
                .AddErrorIfNullOrWhiteSpace(Value, $"{nameof(Name)} is required", nameof(Name))
                .AddErrorIf(Value.Length < MinNameLength, $"{nameof(Name)} must be at least {MinNameLength} characters long", nameof(Name))
                .AddErrorIf(Value.Length > MaxNameLength, $"{nameof(Name)} must be no more than {MaxNameLength} characters", nameof(Name));

            return validationResult;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value.ToLower(CultureInfo.CurrentCulture);
        }
    }
}
