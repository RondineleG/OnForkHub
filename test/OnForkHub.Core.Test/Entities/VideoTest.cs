namespace OnForkHub.Core.Test.Entities;

public class VideoTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add category to video")]
    public void ShouldAddCategoryToVideo()
    {
        var video = Video.Create("Title", "Description", "https://example.com/video", 1L);
        var category = Category.Create("Category", "Category description").Data!;

        video.AddCategory(category);

        video.Categories.Should().Contain(category);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should remove category from video")]
    public void ShouldRemoveCategoryFromVideo()
    {
        var video = Video.Create("Title", "Description", "https://example.com/video", 1L);
        var category = Category.Create("Category", "Category description").Data!;
        video.AddCategory(category);

        video.RemoveCategory(category);

        video.Categories.Should().NotContain(category);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when adding null category")]
    public void ShouldReturnErrorWhenAddingNullCategory()
    {
        var video = Video.Create("Title", "Description", "https://example.com/video", 1L);

        Action act = () => video.AddCategory(null);

        act.Should().Throw<DomainException>().WithMessage("Category cannot be null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when removing null category")]
    public void ShouldReturnErrorWhenRemovingNullCategory()
    {
        var video = Video.Create("Title", "Description", "https://example.com/video", 1L);

        Action act = () => video.RemoveCategory(null);

        act.Should().Throw<DomainException>().WithMessage("Category cannot be null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when updating data with invalid title")]
    public void ShouldReturnErrorWhenUpdatingDataWithInvalidTitle()
    {
        var video = Video.Create("Original Title", "Original Description", "https://original.com/video", 1L);
        var newTitle = "Ti";
        var newDescription = "New description";
        var newUrl = "https://new.com/video";

        var validationResult = video.UpdateCategory(newTitle, newDescription, newUrl);

        validationResult
            .Errors.Should()
            .ContainSingle(error =>
                error.Message == "Title must be at least 3 characters long" && error.Field == nameof(Video.Title)
            );
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when validating video with empty title")]
    public void ShouldReturnErrorWhenValidatingVideoWithEmptyTitle()
    {
        var title = "";
        var description = "Video description";
        var url = "https://example.com/video";
        var userId = 1L;

        var video = Video.Create(title, description, url, userId);
        var validationResult = video.Validate();

        validationResult
            .Errors.Should()
            .ContainSingle(error => error.Message == "Title is required" && error.Field == nameof(Video.Title));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return validation error for title longer than 50 characters")]
    public void ShouldReturnValidationErrorForTitleLongerThan50Characters()
    {
        var title = new string('A', 51); // Título com 51 caracteres
        var description = "Valid Description";
        var url = "https://example.com/video";
        var userId = 1L;

        var video = Video.Create(title, description, url, userId);
        var validationResult = video.Validate();

        validationResult
            .Errors.Should()
            .ContainSingle(error =>
                error.Message == "Title must be no more than 50 characters" && error.Field == nameof(Video.Title)
            );
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully create video with valid data")]
    public void ShouldSuccessfullyCreateVideoWithValidData()
    {
        var title = "Test Video";
        var description = "Video description";
        var url = "https://example.com/video";
        var userId = 1L;

        var video = Video.Create(title, description, url, userId);

        video.Should().NotBeNull();
        video.Title.Should().Be(title);
        video.Description.Should().Be(description);
        video.Url.Value.Should().Be(url);
        video.UserId.Should().Be(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully load video with valid data")]
    public void ShouldSuccessfullyLoadVideoWithValidData()
    {
        var id = 1L;
        var title = "Test Video";
        var description = "Video description";
        var url = "https://example.com/video";
        var userId = 1L;
        var createdAt = DateTime.Now;

        var video = Video.Load(id, title, description, url, userId, createdAt);

        video.Should().NotBeNull();
        video.Id.Should().Be(id);
        video.Title.Should().Be(title);
        video.Description.Should().Be(description);
        video.Url.Value.Should().Be(url);
        video.UserId.Should().Be(userId);
        video.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully update video data with valid data")]
    public void ShouldSuccessfullyUpdateVideoDataWithValidData()
    {
        var video = Video.Create("Original Title", "Original Description", "https://original.com/video", 1L);
        var newTitle = "New Title";
        var newDescription = "New description";
        var newUrl = "https://new.com/video";

        video.UpdateCategory(newTitle, newDescription, newUrl);

        video.Title.Should().Be(newTitle);
        video.Description.Should().Be(newDescription);
        video.Url.Value.Should().Be(newUrl);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should validate video correctly")]
    public void ShouldValidateVideoCorrectly()
    {
        var video = Video.Create("Valid Title", "Valid Description", "https://example.com/video", 1L);

        var validationResult = video.Validate();

        validationResult.Errors.Should().BeEmpty();
    }
}
