using OnForkHub.Persistence.Contexts.Base;
using OnForkHub.Persistence.Repositories;
using UserEntity = OnForkHub.Core.Entities.User;

namespace OnForkHub.Persistence.Test.Repositories;

/// <summary>
/// Unit tests for UserRepositoryEF.
/// Note: Due to EF Core configuration complexities with value objects and relationships,
/// these tests focus on input validation. Full repository integration is covered by integration tests.
/// </summary>
public class UserRepositoryEFTest
{
    private readonly IEntityFrameworkDataContext _context;

    private readonly UserRepositoryEF _userRepository;

    public UserRepositoryEFTest()
    {
        _context = Substitute.For<IEntityFrameworkDataContext>();
        _userRepository = new UserRepositoryEF(_context);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when creating null user")]
    public async Task ShouldThrowArgumentNullExceptionWhenCreatingNullUser()
    {
        var act = () => _userRepository.CreateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Should throw ArgumentNullException when updating null user")]
    public async Task ShouldThrowArgumentNullExceptionWhenUpdatingNullUser()
    {
        var act = () => _userRepository.UpdateAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    private static UserEntity CreateTestUser(string name = "John Silva", string email = "john@email.com")
    {
        var userName = Name.Create(name);
        return UserEntity.Create(userName, email, "hashed_password_123").Data!;
    }
}
