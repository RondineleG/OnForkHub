namespace OnForkHub.Core.Entities;

using OnForkHub.Core.Enums;

/// <summary>
/// Represents a video upload entity with status tracking.
/// </summary>
public class VideoUpload : BaseEntity
{
    protected VideoUpload(Id id, DateTime createdAt, DateTime? updatedAt = null)
        : base(id, createdAt, updatedAt) { }

    private VideoUpload() { }

    public string FileName { get; private set; } = string.Empty;

    public long FileSize { get; private set; }

    public string ContentType { get; private set; } = string.Empty;

    public EVideoUploadStatus Status { get; private set; }

    public string? StoragePath { get; private set; }

    public int ProgressPercentage { get; private set; }

    public DateTime? CompletedAt { get; private set; }

    public Id UserId { get; private set; } = null!;

    public int TotalChunks { get; private set; }

    public int ReceivedChunks { get; private set; }

    public string? ErrorMessage { get; private set; }

    public static RequestResult<VideoUpload> Create(string fileName, long fileSize, string contentType, Id userId, int totalChunks = 1)
    {
        try
        {
            var validation = ValidateInput(fileName, fileSize, contentType, totalChunks);
            if (validation.HasError)
            {
                return RequestResult<VideoUpload>.WithError(validation.ErrorMessage!);
            }

            var upload = new VideoUpload
            {
                FileName = fileName,
                FileSize = fileSize,
                ContentType = contentType,
                Status = EVideoUploadStatus.Pending,
                UserId = userId,
                TotalChunks = totalChunks,
                ReceivedChunks = 0,
                ProgressPercentage = 0,
            };

            upload.ValidateEntityState();
            return RequestResult<VideoUpload>.Success(upload);
        }
        catch (DomainException ex)
        {
            return RequestResult<VideoUpload>.WithError(ex.Message);
        }
    }

    public static RequestResult<VideoUpload> Load(
        Id id,
        string fileName,
        long fileSize,
        string contentType,
        EVideoUploadStatus status,
        Id userId,
        int totalChunks,
        int receivedChunks,
        int progressPercentage,
        string? storagePath,
        DateTime? completedAt,
        string? errorMessage,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        try
        {
            var upload = new VideoUpload(id, createdAt, updatedAt)
            {
                FileName = fileName,
                FileSize = fileSize,
                ContentType = contentType,
                Status = status,
                UserId = userId,
                TotalChunks = totalChunks,
                ReceivedChunks = receivedChunks,
                ProgressPercentage = progressPercentage,
                StoragePath = storagePath,
                CompletedAt = completedAt,
                ErrorMessage = errorMessage,
            };

            upload.ValidateEntityState();
            return RequestResult<VideoUpload>.Success(upload);
        }
        catch (DomainException ex)
        {
            return RequestResult<VideoUpload>.WithError(ex.Message);
        }
    }

    public void MarkAsUploading()
    {
        if (Status != EVideoUploadStatus.Pending)
        {
            throw new DomainException("Only pending uploads can be marked as uploading.");
        }

        Status = EVideoUploadStatus.Uploading;
        Update();
    }

    public void UpdateProgress(int percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new DomainException("Progress percentage must be between 0 and 100.");
        }

        ProgressPercentage = percentage;
    }

    public void IncrementReceivedChunks()
    {
        if (ReceivedChunks >= TotalChunks)
        {
            throw new DomainException("All chunks have already been received.");
        }

        ReceivedChunks++;
        ProgressPercentage = (int)((double)ReceivedChunks / TotalChunks * 100);
        Update();
    }

    public void MarkAsProcessing()
    {
        if (Status != EVideoUploadStatus.Uploading)
        {
            throw new DomainException("Only uploading files can be marked as processing.");
        }

        if (ReceivedChunks != TotalChunks)
        {
            throw new DomainException("All chunks must be received before processing.");
        }

        Status = EVideoUploadStatus.Processing;
        Update();
    }

    public void MarkAsCompleted(string storagePath)
    {
        if (Status != EVideoUploadStatus.Processing)
        {
            throw new DomainException("Only processing uploads can be marked as completed.");
        }

        if (string.IsNullOrWhiteSpace(storagePath))
        {
            throw new DomainException("Storage path is required for completed uploads.");
        }

        Status = EVideoUploadStatus.Completed;
        StoragePath = storagePath;
        CompletedAt = DateTime.UtcNow;
        ProgressPercentage = 100;
        Update();
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = EVideoUploadStatus.Failed;
        ErrorMessage = errorMessage;
        Update();
    }

    protected override void ValidateEntityState()
    {
        base.ValidateEntityState();

        if (string.IsNullOrWhiteSpace(FileName))
        {
            throw new DomainException("File name is required.");
        }

        if (FileSize <= 0)
        {
            throw new DomainException("File size must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(ContentType))
        {
            throw new DomainException("Content type is required.");
        }

        if (UserId is null)
        {
            throw new DomainException("User ID is required.");
        }

        if (TotalChunks <= 0)
        {
            throw new DomainException("Total chunks must be greater than zero.");
        }
    }

    private static ValidationResult ValidateInput(string fileName, long fileSize, string contentType, int totalChunks)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return ValidationResult.Failure("File name is required.");
        }

        if (fileSize <= 0)
        {
            return ValidationResult.Failure("File size must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            return ValidationResult.Failure("Content type is required.");
        }

        if (totalChunks <= 0)
        {
            return ValidationResult.Failure("Total chunks must be greater than zero.");
        }

        return ValidationResult.Success();
    }
}
