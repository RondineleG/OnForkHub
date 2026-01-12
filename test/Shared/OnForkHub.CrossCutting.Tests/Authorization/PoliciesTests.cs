namespace OnForkHub.CrossCutting.Tests.Authorization;

using OnForkHub.CrossCutting.Authorization;

[TestClass]
[TestCategory("Unit")]
public sealed class PoliciesTests
{
    [TestMethod]
    [TestCategory("Authorization")]
    public void RequireAdminPolicyReturnsCorrectValue()
    {
        Policies.RequireAdmin.Should().Be("RequireAdmin");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void RequireModeratorPolicyReturnsCorrectValue()
    {
        Policies.RequireModerator.Should().Be("RequireModerator");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void RequireUserPolicyReturnsCorrectValue()
    {
        Policies.RequireUser.Should().Be("RequireUser");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void RequirePremiumPolicyReturnsCorrectValue()
    {
        Policies.RequirePremium.Should().Be("RequirePremium");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void CanManageContentPolicyReturnsCorrectValue()
    {
        Policies.CanManageContent.Should().Be("CanManageContent");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void CanManageUsersPolicyReturnsCorrectValue()
    {
        Policies.CanManageUsers.Should().Be("CanManageUsers");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void CanUploadVideosPolicyReturnsCorrectValue()
    {
        Policies.CanUploadVideos.Should().Be("CanUploadVideos");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void CanViewPremiumContentPolicyReturnsCorrectValue()
    {
        Policies.CanViewPremiumContent.Should().Be("CanViewPremiumContent");
    }
}
