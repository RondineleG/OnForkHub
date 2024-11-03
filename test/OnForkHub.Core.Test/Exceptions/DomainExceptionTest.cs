using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar DomainException quando a condição for verdadeira")]
    public void DeveLancarDomainExceptionQuandoCondicaoForVerdadeira()
    {
        var message = "Erro de domínio";

        Action action = () => DomainException.ThrowErrorWhen(() => true, message);

        action.Should().Throw<DomainException>().WithMessage(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Não deve lançar DomainException quando a condição for falsa")]
    public void NaoDeveLancarDomainExceptionQuandoCondicaoForFalsa()
    {
        Action action = () => DomainException.ThrowErrorWhen(() => false, "Erro de domínio");

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ThrowWhenInvalid com erro único deve lançar exceção com mensagem do erro")]
    public void ThrowWhenInvalidComErroUnicoDeveLancarExcecaoComMensagemDoErro()
    {
        var result = ValidationResult.Failure("Erro único");

        Action action = () => DomainException.ThrowWhenInvalid(result);

        action.Should().Throw<DomainException>().WithMessage("Erro único");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ThrowWhenInvalid deve lançar exceção com mensagens de múltiplos erros")]
    public void ThrowWhenInvalidDeveLancarExcecaoQuandoHaErros()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        Action action = () => DomainException.ThrowWhenInvalid(result1, result2);

        action.Should().Throw<DomainException>().WithMessage("Erro 1; Erro 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("ThrowWhenInvalid não deve lançar exceção quando não há erros")]
    public void ThrowWhenInvalidNaoDeveLancarExcecaoQuandoNaoHaErros()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        Action action = () => DomainException.ThrowWhenInvalid(result1, result2);

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Validate deve retornar falha quando a condição for verdadeira")]
    public void ValidateDeveRetornarFalhaQuandoCondicaoForVerdadeira()
    {
        var message = "Erro de domínio";
        var result = DomainException.Validate(() => true, message);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Validate deve retornar sucesso quando a condição for falsa")]
    public void ValidateDeveRetornarSucessoQuandoCondicaoForFalsa()
    {
        var message = "Erro de domínio";
        var result = DomainException.Validate(() => false, message);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }
}
