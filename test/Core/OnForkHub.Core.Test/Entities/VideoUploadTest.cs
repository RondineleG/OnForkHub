namespace OnForkHub.Core.Test.Entities;

public class VideoUploadTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create video upload successfully when data is valid")]
    public void ShouldCreateVideoUploadSuccessfullyWhenDataIsValid()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();

        var result = VideoUpload.Create(fileName, fileSize, contentType, userId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.FileName.Should().Be(fileName);
        result.Data.FileSize.Should().Be(fileSize);
        result.Data.ContentType.Should().Be(contentType);
        result.Data.Status.Should().Be(EVideoUploadStatus.Pending);
        result.Data.ProgressPercentage.Should().Be(0);
        result.Data.ReceivedChunks.Should().Be(0);
        result.Data.TotalChunks.Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create video upload with multiple chunks")]
    public void ShouldCreateVideoUploadWithMultipleChunks()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        var totalChunks = 5;

        var result = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.TotalChunks.Should().Be(totalChunks);
    }

    [Theory]
    [InlineData("", 1024, "video/mp4")]
    [InlineData("video.mp4", 0, "video/mp4")]
    [InlineData("video.mp4", 1024, "")]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when creating video upload with invalid data")]
    public void ShouldReturnErrorWhenCreatingVideoUploadWithInvalidData(string fileName, long fileSize, string contentType)
    {
        var userId = Id.Create();

        var result = VideoUpload.Create(fileName, fileSize, contentType, userId);

        result.Status.Should().Be(EResultStatus.HasError);
        result.Data.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should mark as uploading successfully")]
    public void ShouldMarkAsUploadingSuccessfully()
    {
        var upload = CreateValidVideoUpload();

        upload.MarkAsUploading();

        upload.Status.Should().Be(EVideoUploadStatus.Uploading);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when marking non-pending upload as uploading")]
    public void ShouldThrowExceptionWhenMarkingNonPendingUploadAsUploading()
    {
        var upload = CreateValidVideoUpload();
        upload.MarkAsUploading();

        var act = () => upload.MarkAsUploading();

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    [Trait("Category", "Unit")]
    [DisplayName("Should update progress successfully")]
    public void ShouldUpdateProgressSuccessfully(int percentage)
    {
        var upload = CreateValidVideoUpload();

        upload.UpdateProgress(percentage);

        upload.ProgressPercentage.Should().Be(percentage);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when updating progress with invalid percentage")]
    public void ShouldThrowExceptionWhenUpdatingProgressWithInvalidPercentage(int percentage)
    {
        var upload = CreateValidVideoUpload();

        var act = () => upload.UpdateProgress(percentage);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should increment received chunks and update progress")]
    public void ShouldIncrementReceivedChunksAndUpdateProgress()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        var totalChunks = 4;
        var upload = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks).Data!;
        upload.MarkAsUploading();

        upload.IncrementReceivedChunks();

        upload.ReceivedChunks.Should().Be(1);
        upload.ProgressPercentage.Should().Be(25);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when incrementing beyond total chunks")]
    public void ShouldThrowExceptionWhenIncrementingBeyondTotalChunks()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        var totalChunks = 2;
        var upload = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks).Data!;
        upload.MarkAsUploading();
        upload.IncrementReceivedChunks();
        upload.IncrementReceivedChunks();

        var act = () => upload.IncrementReceivedChunks();

        act.Should().Throw<DomainException>().WithMessage("*All chunks have already been received*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should mark as processing successfully when all chunks received")]
    public void ShouldMarkAsProcessingSuccessfullyWhenAllChunksReceived()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        var totalChunks = 2;
        var upload = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks).Data!;
        upload.MarkAsUploading();
        upload.IncrementReceivedChunks();
        upload.IncrementReceivedChunks();

        upload.MarkAsProcessing();

        upload.Status.Should().Be(EVideoUploadStatus.Processing);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when marking as processing without all chunks")]
    public void ShouldThrowExceptionWhenMarkingAsProcessingWithoutAllChunks()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        var totalChunks = 2;
        var upload = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks).Data!;
        upload.MarkAsUploading();
        upload.IncrementReceivedChunks();

        var act = () => upload.MarkAsProcessing();

        act.Should().Throw<DomainException>().WithMessage("*All chunks must be received before processing*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should mark as completed successfully")]
    public void ShouldMarkAsCompletedSuccessfully()
    {
        var storagePath = "/storage/video.mp4";
        var upload = CreateVideoUploadForCompletion();

        upload.MarkAsCompleted(storagePath);

        upload.Status.Should().Be(EVideoUploadStatus.Completed);
        upload.StoragePath.Should().Be(storagePath);
        upload.ProgressPercentage.Should().Be(100);
        upload.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when marking as completed without storage path")]
    public void ShouldThrowExceptionWhenMarkingAsCompletedWithoutStoragePath()
    {
        var upload = CreateVideoUploadForCompletion();

        var act = () => upload.MarkAsCompleted(string.Empty);

        act.Should().Throw<DomainException>().WithMessage("*Storage path is required*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should mark as failed successfully")]
    public void ShouldMarkAsFailedSuccessfully()
    {
        var upload = CreateValidVideoUpload();
        var errorMessage = "Upload failed due to network error";

        upload.MarkAsFailed(errorMessage);

        upload.Status.Should().Be(EVideoUploadStatus.Failed);
        upload.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should load video upload successfully")]
    public void ShouldLoadVideoUploadSuccessfully()
    {
        var id = Id.Create();
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var status = EVideoUploadStatus.Completed;
        var userId = Id.Create();
        var totalChunks = 1;
        var receivedChunks = 1;
        var progressPercentage = 100;
        var storagePath = "/storage/video.mp4";
        var completedAt = DateTime.UtcNow;
        var createdAt = DateTime.UtcNow.AddHours(-1);

        var result = VideoUpload.Load(
            id,
            fileName,
            fileSize,
            contentType,
            status,
            userId,
            totalChunks,
            receivedChunks,
            progressPercentage,
            storagePath,
            completedAt,
            null,
            createdAt
        );

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().EndWith(id.ToString());
        result.Data.FileName.Should().Be(fileName);
        result.Data.Status.Should().Be(status);
    }

    private static VideoUpload CreateValidVideoUpload()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        return VideoUpload.Create(fileName, fileSize, contentType, userId).Data!;
    }

    private static VideoUpload CreateVideoUploadForCompletion()
    {
        var fileName = "video.mp4";
        var fileSize = 1024L;
        var contentType = "video/mp4";
        var userId = Id.Create();
        var totalChunks = 1;
        var upload = VideoUpload.Create(fileName, fileSize, contentType, userId, totalChunks).Data!;
        upload.MarkAsUploading();
        upload.IncrementReceivedChunks();
        upload.MarkAsProcessing();
        return upload;
    }
}
