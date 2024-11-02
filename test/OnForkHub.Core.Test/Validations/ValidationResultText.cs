using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Validations;

public class ValidationResultTests
{
    [Fact]
    public void DeveRetornarSucessoQuandoChamarSuccess()
    {
        var result = ValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void DeveRetornarFalhaQuandoChamarFailure()
    {
        var mensagemErro = "Erro de validação";
        var result = ValidationResult.Failure(mensagemErro);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(mensagemErro);
    }

    [Theory]
    [InlineData(false, "Erro de validação", false)]
    [InlineData(true, "", true)]
    public void DeveValidarCondicaoCorretamente(bool condicao, string mensagem, bool esperadoValido)
    {
        var result = ValidationResult.Validate(!condicao, mensagem);

        result.IsValid.Should().Be(esperadoValido);
        if (!esperadoValido)
            result.ErrorMessage.Should().Be(mensagem);
    }

    [Fact]
    public void DeveLancarExcecaoComMensagemQuandoInvalido()
    {
        var mensagemErro = "Erro de validação";
        var result = ValidationResult.Failure(mensagemErro);

        Action action = () => result.ThrowIfInvalid();

        action.Should().Throw<DomainException>()
            .WithMessage(mensagemErro);
    }

    [Fact]
    public void NaoDeveLancarExcecaoQuandoValido()
    {
        var result = ValidationResult.Success();

        Action action = () => result.ThrowIfInvalid();

        action.Should().NotThrow<DomainException>();
    }

    [Fact]
    public void DeveCombinarValidacoesComErro()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        var resultCombinado = ValidationResult.Combine(result1, result2);

        resultCombinado.IsValid.Should().BeFalse();
        resultCombinado.ErrorMessage.Should().Be("Erro 1; Erro 2");
    }

    [Fact]
    public void DeveRetornarSucessoQuandoTodasAsValidacoesSaoSucesso()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        var resultCombinado = ValidationResult.Combine(result1, result2);

        resultCombinado.IsValid.Should().BeTrue();
        resultCombinado.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void DeveRetornarPrimeiraFalhaQuandoUsarOperadorE()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        var resultFinal = result1 & result2;

        resultFinal.IsValid.Should().BeFalse();
        resultFinal.ErrorMessage.Should().Be("Erro 1");
    }

    [Fact]
    public void DeveRetornarSucessoQuandoUsarOperadorEComTodosValidos()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        var resultFinal = result1 & result2;

        resultFinal.IsValid.Should().BeTrue();
    }
}
