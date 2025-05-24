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

    protected virtual async Task<RequestResult<T>> ExecuteAsync<T>(
        T entity,
        Func<T, Task<RequestResult<T>>> operation,
        IValidationService<T> validationService
    )
        where T : BaseEntity
    {
        if (entity == null)
        {
            var nullValidation = new[] { new RequestValidation(typeof(T).Name, $"{typeof(T).Name} cannot be null") };
            return RequestResult<T>.WithValidations(nullValidation);
        }
        var validationResult = validationService.Validate(entity);
        return validationResult.HasError ? CreateErrorResult<T>(validationResult) : await ExecuteAsync(() => operation(entity));
    }

    private static RequestResult<T> CreateErrorResult<T>(IValidationResult validationResult)
    {
        var validations = validationResult.Errors.Select(error => new RequestValidation(error.Field, error.Message)).ToArray();
        return RequestResult<T>.WithValidations(validations);
    }
}
