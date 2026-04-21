namespace OnForkHub.CrossCutting.Tests.Pagination;

[TestClass]
[TestCategory("Unit")]
public class ValidationResultTests
{
    [TestMethod]
    [TestCategory("Pagination")]
    public void SuccessReturnsValidTrue()
    {
        var result = ValidationResult.Success;

        Assert.IsTrue(result.IsValid);
        Assert.IsNull(result.ErrorMessage);
    }

    [TestMethod]
    [TestCategory("Pagination")]
    public void FailureWithMessageReturnsInvalidWithMessage()
    {
        var message = "Test error message";

        var result = ValidationResult.Failure(message);

        Assert.IsFalse(result.IsValid);
        Assert.AreEqual(message, result.ErrorMessage);
    }
}
