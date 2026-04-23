namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Application.Dtos.Base;

public class PagedResultDtoTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Create should initialize all properties correctly")]
    public void CreateShouldInitializeAllPropertiesCorrectly()
    {
        var items = new List<string> { "Item1", "Item2", "Item3" };
        var page = 2;
        var pageSize = 10;
        var totalItems = 25;

        var result = PagedResultDto<string>.Create(items, page, pageSize, totalItems);

        result.Items.Should().BeEquivalentTo(items);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.TotalItems.Should().Be(totalItems);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("TotalPages should calculate correctly")]
    public void TotalPagesShouldCalculateCorrectly()
    {
        var result = PagedResultDto<string>.Create([], 1, 10, 25);

        result.TotalPages.Should().Be(3);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("TotalPages should return zero when PageSize is zero")]
    public void TotalPagesShouldReturnZeroWhenPageSizeIsZero()
    {
        var result = PagedResultDto<string>.Create([], 1, 0, 25);

        result.TotalPages.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("HasPreviousPage should return false for first page")]
    public void HasPreviousPageShouldReturnFalseForFirstPage()
    {
        var result = PagedResultDto<string>.Create([], 1, 10, 25);

        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("HasPreviousPage should return true for page greater than one")]
    public void HasPreviousPageShouldReturnTrueForPageGreaterThanOne()
    {
        var result = PagedResultDto<string>.Create([], 2, 10, 25);

        result.HasPreviousPage.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("HasNextPage should return true when not on last page")]
    public void HasNextPageShouldReturnTrueWhenNotOnLastPage()
    {
        var result = PagedResultDto<string>.Create([], 1, 10, 25);

        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("HasNextPage should return false when on last page")]
    public void HasNextPageShouldReturnFalseWhenOnLastPage()
    {
        var result = PagedResultDto<string>.Create([], 3, 10, 25);

        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default constructor should initialize Items to empty list")]
    public void DefaultConstructorShouldInitializeItemsToEmptyList()
    {
        var result = new PagedResultDto<string>();

        result.Items.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(100, 10, 10)]
    [InlineData(15, 10, 2)]
    [InlineData(1, 10, 1)]
    [InlineData(10, 10, 1)]
    [DisplayName("TotalPages should calculate correctly for various scenarios")]
    public void TotalPagesShouldCalculateCorrectlyForVariousScenarios(int totalItems, int pageSize, int expectedPages)
    {
        var result = PagedResultDto<string>.Create([], 1, pageSize, totalItems);

        result.TotalPages.Should().Be(expectedPages);
    }
}
