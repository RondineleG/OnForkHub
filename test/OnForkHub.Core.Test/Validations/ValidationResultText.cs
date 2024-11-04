using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Validations;

public class ValidationResultTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro quando condição for verdadeira")]
    public void DeveAdicionarErroQuandoCondicaoForVerdadeira()
    {
        var result = new ValidationResult().AddErrorIf(true, "Erro de condição", "Campo");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Message.Should().Be("Erro de condição");
        result.Errors.First().Field.Should().Be("Campo");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro quando string estiver apenas com espaços")]
    public void DeveAdicionarErroQuandoStringEstiverApenasComEspacos()
    {
        var valor = "   ";
        var result = new ValidationResult().AddErrorIfNullOrWhiteSpace(
            valor,
            "O valor não pode estar em branco",
            "Campo"
        );

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("O valor não pode estar em branco");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro quando string estiver vazia")]
    public void DeveAdicionarErroQuandoStringEstiverVazia()
    {
        var valor = string.Empty;
        var result = new ValidationResult().AddErrorIfNullOrEmpty(valor, "O valor não pode estar vazio", "Campo");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("O valor não pode estar vazio");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro quando valor for nulo")]
    public void DeveAdicionarErroQuandoValorForNulo()
    {
        string? valorNulo = null;
        var result = new ValidationResult().AddErrorIfNull(valorNulo, "O valor não pode ser nulo", "Campo");

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("O valor não pode ser nulo");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve combinar validações com erro")]
    public void DeveCombinarValidacoesComErro()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        var resultCombinado = ValidationResult.Combine(result1, result2);

        resultCombinado.IsValid.Should().BeFalse();
        resultCombinado.ErrorMessage.Should().Be("Erro 1; Erro 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção com mensagem quando inválido")]
    public void DeveLancarExcecaoComMensagemQuandoInvalido()
    {
        var mensagemErro = "Erro de validação";
        var result = ValidationResult.Failure(mensagemErro);

        Action action = result.ThrowIfInvalid;

        action.Should().Throw<DomainException>().WithMessage(mensagemErro);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção ao tentar mesclar com nulo")]
    public void DeveLancarExcecaoQuandoTentarMesclarComNulo()
    {
        var result = new ValidationResult();
        ValidationResult? validate = null;

        Action action = () => result.Merge(validate);

        action.Should().Throw<ArgumentNullException>().WithMessage("*other*");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve manter erros ao mesclar com outro resultado de erro")]
    public void DeveManterErrosQuandoMesclarComOutroResultadoDeErro()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        result1.Merge(result2);

        result1.IsValid.Should().BeFalse();
        result1.Errors.Count.Should().Be(2);
        result1.ErrorMessage.Should().Be("Erro 1; Erro 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve manter sucesso ao mesclar com outro sucesso")]
    public void DeveManterSucessoAoMesclarComOutroSucesso()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        result1.Merge(result2);

        result1.IsValid.Should().BeTrue();
        result1.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro com campo específico ao usar Failure")]
    public void DeveRetornarErroComCampoEspecificoAoUsarFailure()
    {
        var result = ValidationResult.Failure("Erro específico", "CampoEspecifico");

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors.First().Field.Should().Be("CampoEspecifico");
        result.ErrorMessage.Should().Be("Erro específico");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro com múltiplos campos ao adicionar erros")]
    public void DeveRetornarErroComMultiplosCamposQuandoAdicionarErros()
    {
        var erros = new List<(string Message, string Field)>
        {
            ("Erro no campo 1", "Campo1"),
            ("Erro no campo 2", "Campo2"),
        };
        var result = new ValidationResult().AddErrors(erros);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(2);
        result.Errors.Select(e => e.Message).Should().Contain("Erro no campo 1").And.Contain("Erro no campo 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falha ao chamar Failure")]
    public void DeveRetornarFalhaQuandoChamarFailure()
    {
        var mensagemErro = "Erro de validação";
        var result = ValidationResult.Failure(mensagemErro);

        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be(mensagemErro);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar primeira falha ao usar operador E")]
    public void DeveRetornarPrimeiraFalhaQuandoUsarOperadorE()
    {
        var result1 = ValidationResult.Failure("Erro 1");
        var result2 = ValidationResult.Failure("Erro 2");

        var resultFinal = result1 & result2;

        resultFinal.IsValid.Should().BeFalse();
        resultFinal.ErrorMessage.Should().Be("Erro 1");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar sucesso ao chamar Success")]
    public void DeveRetornarSucessoQuandoChamarSuccess()
    {
        var result = ValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar sucesso quando todas as validações são sucesso")]
    public void DeveRetornarSucessoQuandoTodasAsValidacoesSaoSucesso()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        var resultCombinado = ValidationResult.Combine(result1, result2);

        resultCombinado.IsValid.Should().BeTrue();
        resultCombinado.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar sucesso ao usar operador E com todos válidos")]
    public void DeveRetornarSucessoQuandoUsarOperadorEComTodosValidos()
    {
        var result1 = ValidationResult.Success();
        var result2 = ValidationResult.Success();

        var resultFinal = result1 & result2;

        resultFinal.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(false, "Erro de validação", false)]
    [InlineData(true, "", true)]
    [Trait("Category", "Unit")]
    [DisplayName("Deve validar condição corretamente")]
    public void DeveValidarCondicaoCorretamente(bool condicao, string mensagem, bool esperadoValido)
    {
        var result = ValidationResult.Validate(() => !condicao, mensagem);

        result.IsValid.Should().Be(esperadoValido);
        if (!esperadoValido)
        {
            result.ErrorMessage.Should().Be(mensagem);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Não deve adicionar erro quando condição for falsa")]
    public void NaoDeveAdicionarErroQuandoCondicaoForFalsa()
    {
        var result = new ValidationResult().AddErrorIf(false, "Erro de condição", "Campo");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Não deve adicionar erro quando valor não for nulo")]
    public void NaoDeveAdicionarErroQuandoValorNaoForNulo()
    {
        var valor = "texto";
        var result = new ValidationResult().AddErrorIfNull(valor, "O valor não pode ser nulo", "Campo");

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Não deve lançar exceção quando válido")]
    public void NaoDeveLancarExcecaoQuandoValido()
    {
        var result = ValidationResult.Success();

        Action action = result.ThrowIfInvalid;

        action.Should().NotThrow<DomainException>();
    }
}
