namespace OnForkHub.Core.Test.ValueObjects;

public class UrlTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar uma URL válida quando o formato é correto")]
    public void DeveCriarUrlValida()
    {
        var url = Url.Create("https://www.exemplo.com");
        url.Valor.Should().Be("https://www.exemplo.com");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("")]
    [InlineData("   ")]
    [DisplayName("Deve lançar exceção para URL vazia ou apenas com espaços em branco")]
    public void DeveLancarExcecaoParaUrlVazia(string urlValor)
    {
        Action action = () => Url.Create(urlValor);
        action.Should().Throw<DomainException>().WithMessage("URL não pode ser vazia");
    }

    [Theory]
    [Trait("Category", "Unit")]
    [InlineData("htp://endereco_invalido")]
    [InlineData("www.exemplo.com")]
    [InlineData("https://")]
    [InlineData("exemplo")]
    [DisplayName("Deve lançar exceção para formatos inválidos de URL")]
    public void DeveLancarExcecaoParaFormatoInvalido(string urlInvalida)
    {
        Action action = () => Url.Create(urlInvalida);
        action.Should().Throw<DomainException>().WithMessage("URL inválida");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve considerar URLs iguais ignorando maiúsculas e minúsculas")]
    public void DeveConsiderarUrlsIguaisIgnorandoMaiusculasEMinusculas()
    {
        var url1 = Url.Create("https://www.EXEMPLO.com");
        var url2 = Url.Create("https://www.exemplo.com");

        url1.Should().Be(url2);
    }
}
