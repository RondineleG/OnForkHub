namespace OnForkHub.Core.Validations.Videos;

/// <summary>
/// Validator for Video entity.
/// </summary>
public class VideoValidator : IEntityValidator<Video>
{
    /// <inheritdoc/>
    public IValidationResult Validate(Video entity)
    {
        var result = new ValidationResult();
        if (entity == null)
        {
            result.AddError($"{nameof(Video)} cannot be null.", nameof(entity));
            return result;
        }

        if (entity.Title == null)
        {
            result.AddError($"{nameof(Video)} title is required.", nameof(entity.Title));
        }

        if (string.IsNullOrWhiteSpace(entity.Description))
        {
            result.AddError($"{nameof(Video)} description is required.", nameof(entity.Description));
        }
        else if (entity.Description.Length < 5)
        {
            result.AddError($"{nameof(Video)} description must be at least 5 characters.", nameof(entity.Description));
        }
        else if (entity.Description.Length > 200)
        {
            result.AddError($"{nameof(Video)} description cannot exceed 200 characters.", nameof(entity.Description));
        }

        if (entity.Url == null)
        {
            result.AddError($"{nameof(Video)} URL is required.", nameof(entity.Url));
        }

        if (entity.UserId == null)
        {
            result.AddError($"{nameof(Video)} user ID is required.", nameof(entity.UserId));
        }

        return result;
    }

    /// <inheritdoc/>
    public IValidationResult ValidateUpdate(Video entity)
    {
        var result = Validate(entity);
        if (entity?.Id == null)
        {
            result.AddError($"{nameof(Video)} ID is required for updates.", nameof(entity.Id));
        }

        return result;
    }
}
