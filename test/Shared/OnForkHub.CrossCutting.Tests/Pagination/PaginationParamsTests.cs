namespace OnForkHub.CrossCutting.Tests.Pagination;

[TestClass]
[TestCategory("Unit")]
public class PaginationParamsTests
{
    [TestMethod]
    [TestCategory("Pagination")]
    public void ValidateWithValidParamsReturnsSuccess()
    {
        var pagination = new PaginationParams { Page = 1, PageSize = 20 };

        var result = pagination.Validate();

        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void ValidateWithPageLessThanOneReturnsFalse()
    {
        var pagination = new PaginationParams { Page = 0, PageSize = 20 };

        var result = pagination.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage!.Contains("Page number must be >= 1"));
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void ValidateWithPageSizeBelowMinimumReturnsFalse()
    {
        var pagination = new PaginationParams { Page = 1, PageSize = 0 };

        var result = pagination.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.ErrorMessage);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void ValidateWithPageSizeAboveMaximumReturnsFalse()
    {
        var pagination = new PaginationParams { Page = 1, PageSize = 101 };

        var result = pagination.Validate();

        Assert.IsFalse(result.IsValid);
        Assert.IsNotNull(result.ErrorMessage);
        Assert.IsTrue(result.ErrorMessage!.Contains("Page size cannot exceed"));
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void GetSkipCountWithPage1PageSize20Returns0()
    {
        var pagination = new PaginationParams { Page = 1, PageSize = 20 };

        var skipCount = pagination.GetSkipCount();

        Assert.AreEqual(0, skipCount);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void GetSkipCountWithPage2PageSize20Returns20()
    {
        var pagination = new PaginationParams { Page = 2, PageSize = 20 };

        var skipCount = pagination.GetSkipCount();

        Assert.AreEqual(20, skipCount);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void GetSkipCountWithPage5PageSize10Returns40()
    {
        var pagination = new PaginationParams { Page = 5, PageSize = 10 };

        var skipCount = pagination.GetSkipCount();

        Assert.AreEqual(40, skipCount);
    }
}
