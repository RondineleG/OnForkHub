// The .NET Foundation licenses this file to you under the MIT license.

namespace OnForkHub.Core.Test.ValueObjects;

public class UrlTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should allow URLs with query parameters")]
    public void ShouldAllowUrlsWithQueryParameters()
    {
        var url = Url.Create("https://www.example.com?query=test");
        url.Value.Should().Be("https://www.example.com?query=test");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should consider URLs equal ignoring case")]
    public void ShouldConsiderUrlsEqualIgnoringCase()
    {
        var url1 = Url.Create("https://www.EXAMPLE.com");
        var url2 = Url.Create("https://www.example.com");

        url1.Should().Be(url2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should create a valid URL when the format is correct")]
    public void ShouldCreateValidUrl()
    {
        var url = Url.Create("https://www.example.com");
        url.Value.Should().Be("https://www.example.com");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should handle URLs with trailing slashes as equal")]
    public void ShouldHandleUrlsWithTrailingSlashesAsEqual()
    {
        var url1 = Url.Create("https://www.example.com/");
        var url2 = Url.Create("https://www.example.com");

        url1.Should().Be(url2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should return consistent hash code for identical URLs")]
    public void ShouldReturnConsistentHashCodeForIdenticalUrls()
    {
        var url1 = Url.Create("https://www.example.com");
        var url2 = Url.Create("https://www.example.com");

        url1.GetHashCode().Should().Be(url2.GetHashCode());
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [DisplayName("Should throw exception for empty or whitespace-only URL")]
    public void ShouldThrowExceptionForEmptyUrl(string urlValue)
    {
        Action action = () => Url.Create(urlValue);
        action.Should().Throw<DomainException>().WithMessage(UrlResources.UrlRequired);
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("htp://invalid_address")]
    [InlineData("www.example.com")]
    [InlineData("https://")]
    [InlineData("example")]
    [DisplayName("Should throw exception for invalid URL formats")]
    public void ShouldThrowExceptionForInvalidFormat(string invalidUrl)
    {
        Action action = () => Url.Create(invalidUrl);
        action.Should().Throw<DomainException>().WithMessage(UrlResources.UrlInvalid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception when URL scheme is not HTTP or HTTPS")]
    public void ShouldThrowExceptionForInvalidUrlScheme()
    {
        Action action = () => Url.Create("ftp://www.example.com");
        action.Should().Throw<DomainException>().WithMessage(UrlResources.UrlInvalid);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw exception for URL with unsupported characters")]
    public void ShouldThrowExceptionForUrlWithUnsupportedCharacters()
    {
        Action action = () => Url.Create("https://exa<>mple.com");
        action.Should().Throw<DomainException>().WithMessage(UrlResources.UrlInvalid);
    }
}
