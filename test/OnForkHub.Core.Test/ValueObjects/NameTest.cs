namespace OnForkHub.Core.Test.ValueObjects;

public class NameTest
{
    [Theory]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar nome válido")]
    [InlineData("Ana")]
    [InlineData("João Pedro")]
    [InlineData("JoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoao")]
    [InlineData("João.#2134AXca!.|_''\"\\=+-12334#$90xz>;")]
    public void DeveCriarNomeValido(string name)
    {
        Action act = () => Name.Create(name);

        act.Should().NotThrow();
    }

    [Theory]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar nome inválido")]
    [InlineData("Zé")]
    [InlineData("")]
    [InlineData("JoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoaoJoao")]
    public void DeveCriarNomeInvalido(string name)
    {
        Action act = () => Name.Create(name);

        act.Should().Throw<DomainException>();
    }
}
