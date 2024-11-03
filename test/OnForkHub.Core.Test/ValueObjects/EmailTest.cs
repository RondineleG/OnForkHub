namespace OnForkHub.Core.Test.ValueObjects;

public class EmailTest
{
    [Fact]
    [Trait("Category", "Unit")]
    public void DeveCriarEmailValido()
    {
        var email = Email.Create("teste@email.com");

        email.Should().NotBeNull();
        email.Value.Should().Be("teste@email.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("emailinvalido")]
    [InlineData("email@")]
    [InlineData("@dominio.com")]
    [Trait("Category", "Unit")]
    public void DeveLancarExcecaoQuandoEmailInvalido(string emailInvalido)
    {
        Action act = () => Email.Create(emailInvalido);

        act.Should().Throw<DomainException>();
    }
}
