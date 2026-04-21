namespace OnForkHub.CrossCutting.Tests.Pagination;

[TestClass]
[TestCategory("Unit")]
public class PaginationHelperTests
{
    [TestMethod]
    [TestCategory("Pagination")]
    public void ParseFromQueryWithValidValuesReturnsParsedParams()
    {
        var result = PaginationHelper.ParseFromQuery("2", "30");

        Assert.AreEqual(2, result.Page);
        Assert.AreEqual(30, result.PageSize);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void ParseFromQueryWithNullValuesReturnsDefaults()
    {
        var result = PaginationHelper.ParseFromQuery(null, null);

        Assert.AreEqual(1, result.Page);
        Assert.AreEqual(20, result.PageSize);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void ParseFromQueryWithPageSizeExceedingMaxClampsToMax()
    {
        var result = PaginationHelper.ParseFromQuery("1", "200");

        Assert.AreEqual(100, result.PageSize);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void CreateResponseWithValidParamsCreatesCorrectResponse()
    {
        var items = new[] { "item1", "item2", "item3" };

        var response = PaginationHelper.CreateResponse(items, 1, 3, 10);

        Assert.AreEqual(1, response.CurrentPage);
        Assert.AreEqual(3, response.PageSize);
        Assert.AreEqual(10L, response.TotalItems);
        Assert.AreEqual(4L, response.TotalPages);
        Assert.IsTrue(response.HasNextPage);
        Assert.IsFalse(response.HasPreviousPage);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void CreateResponseOnLastPageSetsCorrectHasNextPage()
    {
        var items = new[] { "item1", "item2" };

        var response = PaginationHelper.CreateResponse(items, 2, 5, 10);

        Assert.AreEqual(2L, response.TotalPages);
        Assert.IsFalse(response.HasNextPage);
        Assert.IsTrue(response.HasPreviousPage);
    }
}
