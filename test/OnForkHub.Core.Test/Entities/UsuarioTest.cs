namespace OnForkHub.Core.Test.Entities;

public class UsuarioTest
{
    [Fact]
    public void DeveCriarUsuarioComSucesso()
    {
        var usuario = Usuario.Create("João Silva", "joao@email.com");

        usuario.Should().NotBeNull();
        usuario.Nome.Should().Be("João Silva");
        usuario.Email.Value.Should().Be("joao@email.com");
        usuario.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        usuario.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Jo")]
    public void DeveLancarExcecaoQuandoNomeInvalido(string nomeInvalido)
    {
        Action act = () => Usuario.Create(nomeInvalido, "email@test.com");

        act.Should().Throw<DomainException>();
    }
}