using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Exceptions;

public class DomainExceptionTests
{
    [Fact(DisplayName = nameof(DeveLancarDomainExceptionQuandoCondicaoForVerdadeira))]
    [Trait("Domain", "Exceptions - DomainException")]
    public void DeveLancarDomainExceptionQuandoCondicaoForVerdadeira()
    {
        var message = "Erro de domínio";

        Action action = () => DomainException.ThrowErrorWhen(() => true, message);

        action.Should().Throw<DomainException>().WithMessage(message);
    }

    [Fact]
    public void NaoDeveLancarDomainExceptionQuandoCondicaoForFalsa()
    {
        Action action = () => DomainException.ThrowErrorWhen(() => false, "Erro de domínio");

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    public void ThrowWhenInvalidComErroUnicoDeveLancarExcecaoComMensagemDoErro()
    {
        var result = ValidationResult.Failure("Erro único");

        Action action = () => DomainException.ThrowWhenInvalid(result);

        action.Should().Throw<DomainException>().WithMessage("Erro único");
    }

    [Fact]
    public void ThrowWhenInvalidDeveLancarExcecaoQuandoHaErros()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        Action action = () => DomainException.ThrowWhenInvalid(result1, result2);

        action.Should().Throw<DomainException>().WithMessage("Erro 1; Erro 2");
    }

    [Fact]
    public void ThrowWhenInvalidNaoDeveLancarExcecaoQuandoNaoHaErros()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        Action action = () => DomainException.ThrowWhenInvalid(result1, result2);

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    public void ValidateDeveRetornarFalhaQuandoCondicaoForVerdadeira()
    {
        var message = "Erro de domínio";
        var result = DomainException.Validate(() => true, message);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(message);
    }

    [Fact]
    public void ValidateDeveRetornarSucessoQuandoCondicaoForFalsa()
    {
        var message = "Erro de domínio";
        var result = DomainException.Validate(() => false, message);

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }
}
