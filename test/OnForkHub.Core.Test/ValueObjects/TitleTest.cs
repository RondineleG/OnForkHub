namespace OnForkHub.Core.Test.ValueObjects;

public class TitleTest
{
    [Theory]
    [InlineData("teste")]
    [InlineData("alo")]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvw")]
    [DisplayName("Criar titulo com dados validos")]
    public void CriarTituloComDadosValidos(string title)
    {
        Action act = () => Title.Create(title);
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("")]
    [InlineData("aa")]
    [InlineData("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")]
    [DisplayName("Criar título com dados inválidos")]
    public void CriarTituloComDadosInvalidosDeveLancarExcecao(string title)
    {
        Action act = () => Title.Create(title);
        act.Should().Throw<DomainException>();
    }
}
