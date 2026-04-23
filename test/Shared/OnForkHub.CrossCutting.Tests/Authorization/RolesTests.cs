namespace OnForkHub.CrossCutting.Tests.Authorization;

using OnForkHub.CrossCutting.Authorization;

[TestClass]
[TestCategory("Unit")]
public sealed class RolesTests
{
    [TestMethod]
    [TestCategory("Authorization")]
    public void AdminRoleReturnsCorrectValue()
    {
        Roles.Admin.Should().Be("Admin");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void ModeratorRoleReturnsCorrectValue()
    {
        Roles.Moderator.Should().Be("Moderator");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void UserRoleReturnsCorrectValue()
    {
        Roles.User.Should().Be("User");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void PremiumRoleReturnsCorrectValue()
    {
        Roles.Premium.Should().Be("Premium");
    }

    [TestMethod]
    [TestCategory("Authorization")]
    public void AllRolesContainsAllDefinedRoles()
    {
        var allRoles = Roles.All;

        allRoles.Should().Contain(Roles.Admin);
        allRoles.Should().Contain(Roles.Moderator);
        allRoles.Should().Contain(Roles.User);
        allRoles.Should().Contain(Roles.Premium);
        allRoles.Should().HaveCount(4);
    }

    [TestMethod]
    [TestCategory("Authorization")]
    [DataRow("Admin", true)]
    [DataRow("ADMIN", true)]
    [DataRow("admin", true)]
    [DataRow("Moderator", true)]
    [DataRow("User", true)]
    [DataRow("Premium", true)]
    [DataRow("Invalid", false)]
    [DataRow("", false)]
    [DataRow("SuperAdmin", false)]
    public void IsValidReturnsCorrectResultForRole(string role, bool expected)
    {
        var result = Roles.IsValid(role);

        result.Should().Be(expected);
    }
}
