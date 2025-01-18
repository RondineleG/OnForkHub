namespace OnForkHub.Core.Validations;

public abstract class ValidationService<T>(IValidationBuilder<T> builder, IEntityValidator<T> validator) : IValidationService<T>
    where T : BaseEntity
{
    private readonly List<IValidationRule<T>> _customRules = [];

    private readonly List<Action<ValidationErrorMessage>> _errorHandlers = [];

    private readonly List<Func<T, ValidationResult>> _validations = [];

    public IValidationBuilder<T> Builder { get; } = builder ?? throw new ArgumentNullException(nameof(builder));

    public IEntityValidator<T> Validator { get; } = validator ?? throw new ArgumentNullException(nameof(validator));

    public IValidationService<T> AddRule(IValidationRule<T> rule)
    {
        _customRules.Add(rule);
        return this;
    }

    public IValidationService<T> AddValidation(Func<T, ValidationResult> validation)
    {
        _validations.Add(validation);
        return this;
    }

    public virtual IValidationResult Validate(T entity)
    {
        try
        {
            if (entity == null)
            {
                return ValidationResult.Failure($"{typeof(T).Name} cannot be null", nameof(T));
            }

            var result = new ValidationResult();

            result.Merge(Validator.Validate(entity));

            foreach (var rule in _customRules)
            {
                result.Merge(rule.Validate(entity));
            }

            foreach (var validation in _validations)
            {
                result.Merge(validation(entity));
            }

            if (result.HasError)
            {
                foreach (var error in result.Errors)
                {
                    foreach (var handler in _errorHandlers)
                    {
                        handler(error);
                    }
                }
            }

            return result;
        }
        finally
        {
            Builder.Validate();
        }
    }

    public virtual IValidationResult ValidateUpdate(T entity)
    {
        var result = Validate(entity);

        if (entity?.Id is null)
        {
            result.AddError($"{typeof(T).Name} ID is required for updates", nameof(entity.Id));
        }

        return result;
    }

    public IValidationService<T> WithErrorHandler(Action<ValidationErrorMessage> errorHandler)
    {
        _errorHandlers.Add(errorHandler);
        return this;
    }

    protected virtual ValidationResult ValidateProperty<TProperty>(
        T entity,
        Expression<Func<T, TProperty>> propertyExpression,
        Action<IValidationBuilder<T>> validationAction
    )
    {
        var propertyName = GetPropertyName(propertyExpression);
        var propertyValue = propertyExpression.Compile()(entity);

        Builder.WithField(propertyName, propertyValue);
        validationAction(Builder);

        return (ValidationResult)Builder.Validate();
    }

    private static string GetPropertyName<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
    {
        return propertyExpression.Body is MemberExpression memberExpression
            ? memberExpression.Member.Name
            : throw new ArgumentException("Expression must be a property access", nameof(propertyExpression));
    }
}