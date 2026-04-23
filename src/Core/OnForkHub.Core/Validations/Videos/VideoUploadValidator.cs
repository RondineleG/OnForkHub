namespace OnForkHub.Core.Validations.Videos;

using OnForkHub.Core.Entities;
using OnForkHub.Core.Interfaces.Validations;

/// <summary>
/// Validator for VideoUpload entity.
/// </summary>
public class VideoUploadValidator : IEntityValidator<VideoUpload>
{
    private const long MaxFileSize = 100 * 1024 * 1024; // 100MB
    private static readonly string[] AllowedExtensions = [".mp4", ".webm", ".mov"];

    /// <inheritdoc/>
    public IValidationResult Validate(VideoUpload entity)
    {
        var result = new ValidationResult();
        if (entity == null)
        {
            result.AddError($"{nameof(VideoUpload)} cannot be null.", nameof(entity));
            return result;
        }

        if (string.IsNullOrWhiteSpace(entity.FileName))
        {
            result.AddError("File name is required.", nameof(entity.FileName));
        }
        else
        {
            var extension = Path.GetExtension(entity.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                result.AddError(
                    $"File extension {extension} is not allowed. Allowed: {string.Join(", ", AllowedExtensions)}",
                    nameof(entity.FileName)
                );
            }
        }

        if (entity.FileSize <= 0)
        {
            result.AddError("File size must be greater than zero.", nameof(entity.FileSize));
        }
        else if (entity.FileSize > MaxFileSize)
        {
            result.AddError($"File size exceeds the maximum limit of {MaxFileSize / 1024 / 1024}MB.", nameof(entity.FileSize));
        }

        if (string.IsNullOrWhiteSpace(entity.ContentType))
        {
            result.AddError("Content type is required.", nameof(entity.ContentType));
        }

        if (entity.UserId == null)
        {
            result.AddError("User ID is required.", nameof(entity.UserId));
        }

        if (entity.TotalChunks <= 0)
        {
            result.AddError("Total chunks must be greater than zero.", nameof(entity.TotalChunks));
        }

        return result;
    }

    /// <inheritdoc/>
    public IValidationResult ValidateUpdate(VideoUpload entity)
    {
        var result = Validate(entity);
        if (entity?.Id == null)
        {
            result.AddError($"{nameof(VideoUpload)} ID is required for updates.", nameof(entity.Id));
        }

        return result;
    }
}
