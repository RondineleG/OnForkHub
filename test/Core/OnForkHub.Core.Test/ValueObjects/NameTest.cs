namespace OnForkHub.Core.Test.ValueObjects;

public class NameTest
{
    [Theory]
    [Trait("Category", "Unit")]
    [DisplayName("Should create a valid name")]
    [InlineData("Ana")]
    [InlineData("John Peter")]
    [InlineData("JohnJohnJohnJohnJohnJohnJohnJohnJohnJohn")]
    [InlineData("John.#2134AXca!.|_''\"\\=+-12334#$90xz>;")]
    public void ShouldCreateValidName(string name)
    {
        Action act = () => Name.Create(name);

        act.Should().NotThrow();
    }
}
