namespace OnForkHub.Core.Test.Entities;

public class UsuarioTests
{
    [Fact]
    public void DeveCriarUsuarioComSucesso()
    {
        // Arrange & Act
        var usuario = Usuario.Create("João Silva", "joao@email.com");

        // Assert
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
        // Arrange & Act
        Action act = () => Usuario.Create(nomeInvalido, "email@test.com");

        // Assert
        act.Should().Throw<DomainException>();
    }
}