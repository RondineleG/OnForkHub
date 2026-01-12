namespace OnForkHub.CrossCutting.Tests.Authorization;

using OnForkHub.CrossCutting.Authorization;
using OnForkHub.CrossCutting.Authorization.Requirements;

[TestClass]
[TestCategory("Unit")]
public sealed class RoleRequirementTests
{
    [TestMethod]
    [TestCategory("Authorization")]
    public void ConstructorWithValidRolesStoresRoles()
    {
        var requirement = new RoleRequirement(Roles.Admin, Roles.Moderator);

        requirement.AllowedRoles.Should().Contain(Roles.Admin);
        requirement.AllowedRoles.Should().Contain(Roles.Moderator);
        requirement.AllowedRoles.Should().HaveCount(2);
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void ConstructorWithNullRolesThrowsArgumentNullException()
    {
        Action act = () => new RoleRequirement(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void ConstructorWithEmptyRolesCreatesEmptyList()
    {
        var requirement = new RoleRequirement();

        requirement.AllowedRoles.Should().BeEmpty();
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void ConstructorWithSingleRoleStoresSingleRole()
    {
        var requirement = new RoleRequirement(Roles.Admin);

        requirement.AllowedRoles.Should().ContainSingle();
        requirement.AllowedRoles[0].Should().Be(Roles.Admin);
    }
}
