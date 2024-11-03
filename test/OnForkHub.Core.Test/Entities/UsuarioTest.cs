namespace OnForkHub.Core.Test.Entities;

public class UsuarioTest
{
    [Fact]
    [Trait("Category", "Unit")]
    public void DeveCriarUsuarioComSucesso()
    {
        var usuario = Usuario.Create("Jo�o Silva", "joao@email.com");

        usuario.Should().NotBeNull();
        usuario.Nome.Should().Be("Jo�o Silva");
        usuario.Email.Value.Should().Be("joao@email.com");
        usuario.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        usuario.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Jo")]
    [Trait("Category", "Unit")]
    public void DeveLancarExcecaoQuandoNomeInvalido(string nomeInvalido)
    {
        Action act = () => Usuario.Create(nomeInvalido, "email@test.com");

        act.Should().Throw<DomainException>();
    }
}
