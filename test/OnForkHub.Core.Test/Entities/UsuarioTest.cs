namespace OnForkHub.Core.Test.Entities;

public class UsuarioTest
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar usuário com sucesso")]
    public void DeveCriarUsuarioComSucesso()
    {
        var usuario = Usuario.Create("João Silva", "joao@email.com");

        usuario.Should().NotBeNull();
        usuario.Name.Value.Should().Be("João Silva");
        usuario.Email.Value.Should().Be("joao@email.com");
        usuario.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        usuario.UpdatedAt.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Jo")]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção ao criar usuário com nome inválido")]
    public void DeveLancarExcecaoQuandoNomeInvalido(string nomeInvalido)
    {
        Action act = () => Usuario.Create(nomeInvalido, "email@test.com");

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("email@")]
    [InlineData("@domain.com")]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção ao criar usuário com email inválido")]
    public void DeveLancarExcecaoQuandoEmailInvalido(string emailInvalido)
    {
        Action act = () => Usuario.Create("João Silva", emailInvalido);

        act.Should().Throw<DomainException>().WithMessage("Email inválido");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve atualizar nome e email do usuário com sucesso")]
    public void DeveAtualizarNomeEEmailDoUsuarioComSucesso()
    {
        var usuario = Usuario.Create("João Silva", "joao@email.com");
        usuario.AtualizarNome("João Pereira");
        usuario.AtualizarEmail("joao.pereira@email.com");

        usuario.Name.Value.Should().Be("João Pereira");
        usuario.Email.Value.Should().Be("joao.pereira@email.com");
        usuario.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção ao atualizar usuário com nome inválido")]
    public void DeveLancarExcecaoQuandoAtualizarNomeInvalido()
    {
        var usuario = Usuario.Create("João Silva", "joao@email.com");

        Action act = () => usuario.AtualizarNome("Jo");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção ao atualizar usuário com email inválido")]
    public void DeveLancarExcecaoQuandoAtualizarEmailInvalido()
    {
        var usuario = Usuario.Create("João Silva", "joao@email.com");

        Action act = () => usuario.AtualizarEmail("email@");

        act.Should().Throw<DomainException>().WithMessage("Email inválido");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve manter CreatedAt inalterado após atualização")]
    public void DeveManterCreatedAtInalteradoAposAtualizacao()
    {
        var usuario = Usuario.Create("João Silva", "joao@email.com");
        var dataCriacao = usuario.CreatedAt;

        usuario.AtualizarNome("João Pereira");

        usuario.CreatedAt.Should().Be(dataCriacao);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção ao tentar criar usuário com email vazio")]
    public void DeveLancarExcecaoQuandoEmailVazio()
    {
        Action act = () => Usuario.Create("João Silva", "");

        act.Should().Throw<DomainException>().WithMessage("Email não pode ser vazio");
    }
}
