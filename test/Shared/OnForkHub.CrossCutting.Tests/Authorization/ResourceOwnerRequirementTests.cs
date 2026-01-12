namespace OnForkHub.CrossCutting.Tests.Authorization;

using OnForkHub.CrossCutting.Authorization.Requirements;

[TestClass]
[TestCategory("Unit")]
public sealed class ResourceOwnerRequirementTests
{
    [TestMethod]
    [TestCategory("Authorization")]
    public void DefaultConstructorSetsAllowAdminOverrideToTrue()
    {
        var requirement = new ResourceOwnerRequirement();

        requirement.AllowAdminOverride.Should().BeTrue();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void ConstructorWithFalseSetsAllowAdminOverrideToFalse()
    {
        var requirement = new ResourceOwnerRequirement(allowAdminOverride: false);

        requirement.AllowAdminOverride.Should().BeFalse();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void ConstructorWithTrueSetsAllowAdminOverrideToTrue()
    {
        var requirement = new ResourceOwnerRequirement(allowAdminOverride: true);

        requirement.AllowAdminOverride.Should().BeTrue();
    }
}
