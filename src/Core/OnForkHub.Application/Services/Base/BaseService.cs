namespace OnForkHub.Application.Services.Base;

public abstract class BaseService
{
    protected virtual async Task<RequestResult<T>> ExecuteAsync<T>(
        Func<Task<RequestResult<T>>> operation,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await operation();
        }
        catch (CustomResultException ex)
        {
            return RequestResult<T>.WithError(ex.CustomResult.Message);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RequestResult<T>.WithError("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return RequestResult<T>.WithError($"Error processing operation: {ex.Message}");
        }
    }

    protected async Task<RequestResult<T>> ExecuteAsync<T>(
        T entity,
        Func<T, Task<RequestResult<T>>> operation,
        Func<T, ValidationResult> validationFunc
    )
        where T : class
    {
        if (entity == null)
        {
            var nullValidation = new[] { new RequestValidation(typeof(T).Name, $"{typeof(T).Name} cannot be null") };
            return RequestResult<T>.WithValidations(nullValidation);
        }

        var validationResult = validationFunc(entity);
        return validationResult.HasError ? CreateErrorResult<T>(validationResult) : await ExecuteAsync(() => operation(entity));
    }

    protected async Task<RequestResult<T>> ExecuteWithValidationAsync<T>(
        T entity,
        Func<T, Task<RequestResult<T>>> operation,
        IValidationService<T> validationService,
        bool isUpdate = false
    )
        where T : BaseEntity
    {
        if (entity == null)
        {
            var nullValidation = new[] { new RequestValidation(typeof(T).Name, $"{typeof(T).Name} cannot be null") };
            return RequestResult<T>.WithValidations(nullValidation);
        }

        var validationResult = isUpdate ? validationService.ValidateUpdate(entity) : validationService.Validate(entity);

        return validationResult.HasError ? CreateErrorResult<T>(validationResult) : await ExecuteAsync(() => operation(entity));
    }

    protected async Task<RequestResult<IEnumerable<T>>> ExecuteBatchWithValidationAsync<T>(
        IEnumerable<T> entities,
        Func<IEnumerable<T>, Task<RequestResult<IEnumerable<T>>>> operation,
        IValidationService<T> validationService
    )
        where T : BaseEntity
    {
        if (entities == null || !entities.Any())
        {
            var nullValidation = new[] { new RequestValidation(typeof(T).Name, $"No {typeof(T).Name} entities provided") };
            return RequestResult<IEnumerable<T>>.WithValidations(nullValidation);
        }

        var validationResults = entities.Select(validationService.Validate);
        var errors = validationResults.SelectMany(result => result.Errors).ToList();

        return errors.Count != 0 ? CreateBatchErrorResult<T>(errors) : await ExecuteAsync(() => operation(entities));
    }

    private static RequestResult<T> CreateErrorResult<T>(IValidationResult validationResult)
    {
        var validations = validationResult.Errors.Select(error => new RequestValidation(error.Field, error.Message)).ToArray();

        return RequestResult<T>.WithValidations(validations);
    }

    private static RequestResult<IEnumerable<T>> CreateBatchErrorResult<T>(IEnumerable<ValidationErrorMessage> errors)
    {
        var validations = errors.Select(error => new RequestValidation(error.Field, error.Message)).ToArray();

        return RequestResult<IEnumerable<T>>.WithValidations(validations);
    }
}