namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Application.Dtos.Video.Request;

public class VideoSearchRequestDtoTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default values should be set correctly")]
    public void DefaultValuesShouldBeSetCorrectly()
    {
        var dto = new VideoSearchRequestDto();

        dto.SearchTerm.Should().BeNull();
        dto.CategoryId.Should().BeNull();
        dto.UserId.Should().BeNull();
        dto.FromDate.Should().BeNull();
        dto.ToDate.Should().BeNull();
        dto.SortBy.Should().Be(VideoSortField.CreatedAt);
        dto.SortDescending.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set SearchTerm property")]
    public void ShouldSetSearchTermProperty()
    {
        var dto = new VideoSearchRequestDto { SearchTerm = "Test Search" };

        dto.SearchTerm.Should().Be("Test Search");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set CategoryId property")]
    public void ShouldSetCategoryIdProperty()
    {
        var dto = new VideoSearchRequestDto { CategoryId = 123 };

        dto.CategoryId.Should().Be(123);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set UserId property")]
    public void ShouldSetUserIdProperty()
    {
        var dto = new VideoSearchRequestDto { UserId = "user-456" };

        dto.UserId.Should().Be("user-456");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set FromDate property")]
    public void ShouldSetFromDateProperty()
    {
        var date = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var dto = new VideoSearchRequestDto { FromDate = date };

        dto.FromDate.Should().Be(date);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set ToDate property")]
    public void ShouldSetToDateProperty()
    {
        var date = new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc);
        var dto = new VideoSearchRequestDto { ToDate = date };

        dto.ToDate.Should().Be(date);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(VideoSortField.CreatedAt)]
    [InlineData(VideoSortField.Title)]
    [InlineData(VideoSortField.UpdatedAt)]
    [DisplayName("Should set SortBy property for all values")]
    public void ShouldSetSortByPropertyForAllValues(VideoSortField sortField)
    {
        var dto = new VideoSearchRequestDto { SortBy = sortField };

        dto.SortBy.Should().Be(sortField);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(true)]
    [InlineData(false)]
    [DisplayName("Should set SortDescending property")]
    public void ShouldSetSortDescendingProperty(bool descending)
    {
        var dto = new VideoSearchRequestDto { SortDescending = descending };

        dto.SortDescending.Should().Be(descending);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("VideoSortField enum should have correct values")]
    public void VideoSortFieldEnumShouldHaveCorrectValues()
    {
        ((int)VideoSortField.CreatedAt).Should().Be(0);
        ((int)VideoSortField.Title).Should().Be(1);
        ((int)VideoSortField.UpdatedAt).Should().Be(2);
    }
}
