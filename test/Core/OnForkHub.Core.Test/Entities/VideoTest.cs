// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Test.Entities;

public class VideoTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should add category to video")]
    public void ShouldAddCategoryToVideo()
    {
        var name = Name.Create("Category");
        var video = Video.Create("Title", "Description", "https://example.com/video", Id.Create()).Data!;
        var category = Category.Create(name, "Category description").Data!;

        var result = video.AddCategory(category);

        result.Status.Should().Be(EResultStatus.Success);
        video.Categories.Should().Contain(category);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should remove category from video")]
    public void ShouldRemoveCategoryFromVideo()
    {
        var name = Name.Create("Category");
        var video = Video.Create("Title", "Description", "https://example.com/video", Id.Create()).Data!;
        var category = Category.Create(name, "Category description").Data!;
        video.AddCategory(category);

        var result = video.RemoveCategory(category);

        result.Status.Should().Be(EResultStatus.Success);
        video.Categories.Should().NotContain(category);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when adding null category")]
    public void ShouldReturnErrorWhenAddingNullCategory()
    {
        var video = Video.Create("Title", "Description", "https://example.com/video", Id.Create()).Data!;

        var result = video.AddCategory(null!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError!.Description.Should().Be(VideoResources.CategoryCannotBeNull);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return error when removing null category")]
    public void ShouldReturnErrorWhenRemovingNullCategory()
    {
        var video = Video.Create("Title", "Description", "https://example.com/video", Id.Create()).Data!;

        var result = video.RemoveCategory(null!);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError!.Description.Should().Be(VideoResources.CategoryCannotBeNull);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully create video with valid data")]
    public void ShouldSuccessfullyCreateVideoWithValidData()
    {
        var title = "Test Video";
        var description = "Video description";
        var url = "https://example.com/video";
        var userId = Id.Create();

        var result = Video.Create(title, description, url, userId);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Title.Value.Should().Be(title);
        result.Data.Description.Should().Be(description);
        result.Data.Url.Value.Should().Be(url);
        result.Data.UserId.Should().Be(userId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should successfully update video data with valid data")]
    public void ShouldSuccessfullyUpdateVideoDataWithValidData()
    {
        var video = Video.Create("Original Title", "Original Description", "https://original.com/video", Id.Create()).Data!;
        var newTitle = "New Title";
        var newDescription = "New description";
        var newUrl = "https://new.com/video";

        var result = video.UpdateVideo(newTitle, newDescription, newUrl);

        result.Status.Should().Be(EResultStatus.Success);
        video.Title.Value.Should().Be(newTitle);
        video.Description.Should().Be(newDescription);
        video.Url.Value.Should().Be(newUrl);
    }
}
