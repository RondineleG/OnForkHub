namespace OnForkHub.Application.Test.Dtos;

using OnForkHub.Application.Dtos.Category.Request;

public class CategorySearchRequestTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Default values should be set correctly")]
    public void DefaultValuesShouldBeSetCorrectly()
    {
        var dto = new CategorySearchRequest();

        dto.SearchTerm.Should().BeNull();
        dto.SortBy.Should().Be(CategorySortField.Name);
        dto.SortDescending.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should set SearchTerm property")]
    public void ShouldSetSearchTermProperty()
    {
        var dto = new CategorySearchRequest { SearchTerm = "Test Category" };

        dto.SearchTerm.Should().Be("Test Category");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(CategorySortField.Name)]
    [InlineData(CategorySortField.CreatedAt)]
    [DisplayName("Should set SortBy property for all values")]
    public void ShouldSetSortByPropertyForAllValues(CategorySortField sortField)
    {
        var dto = new CategorySearchRequest { SortBy = sortField };

        dto.SortBy.Should().Be(sortField);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData(true)]
    [InlineData(false)]
    [DisplayName("Should set SortDescending property")]
    public void ShouldSetSortDescendingProperty(bool descending)
    {
        var dto = new CategorySearchRequest { SortDescending = descending };

        dto.SortDescending.Should().Be(descending);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("CategorySortField enum should have correct values")]
    public void CategorySortFieldEnumShouldHaveCorrectValues()
    {
        ((int)CategorySortField.Name).Should().Be(0);
        ((int)CategorySortField.CreatedAt).Should().Be(1);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should inherit from PaginationRequest")]
    public void ShouldInheritFromPaginationRequest()
    {
        var dto = new CategorySearchRequest { Page = 5, ItemsPerPage = 25 };

        dto.Page.Should().Be(5);
        dto.ItemsPerPage.Should().Be(25);
    }
}
