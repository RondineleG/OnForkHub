namespace OnForkHub.Core.Test.ValueObjects;

public class EmailTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar um email válido quando o formato é correto")]
    public void DeveCriarEmailValido()
    {
        var email = Email.Create("exemplo@dominio.com");
        email.Value.Should().Be("exemplo@dominio.com");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [DisplayName("Deve lançar exceção para email vazio ou apenas com espaços em branco")]
    public void DeveLancarExcecaoParaEmailVazio(string emailValue)
    {
        Action action = () => Email.Create(emailValue);
        action.Should().Throw<DomainException>().WithMessage("Email não pode ser vazio");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("emailinvalido.com")]
    [InlineData("invalido@")]
    [InlineData("invalido@com.")]
    [InlineData("invalido@com")]
    [InlineData("@dominio.com")]
    [DisplayName("Deve lançar exceção para formatos inválidos de email")]
    public void DeveLancarExcecaoParaFormatoInvalido(string emailInvalido)
    {
        Action action = () => Email.Create(emailInvalido);
        action.Should().Throw<DomainException>().WithMessage("Email inválido");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("usuario+teste@dominio.com")]
    [InlineData("usuario.nome@sub.dominio.co.uk")]
    [InlineData("usuario@example.io")]
    [DisplayName("Deve aceitar emails válidos com caracteres especiais permitidos")]
    public void DeveAceitarEmailValidoComCaracteresEspeciais(string emailValue)
    {
        var email = Email.Create(emailValue);
        email.Value.Should().Be(emailValue);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve considerar iguais emails com letras maiúsculas e minúsculas")]
    public void DeveConsiderarEmailsIguaisIgnorandoMaiusculasEMinusculas()
    {
        var email1 = Email.Create("teste@DOMINIO.com");
        var email2 = Email.Create("TESTE@dominio.com");

        email1.Should().Be(email2);
    }
}
