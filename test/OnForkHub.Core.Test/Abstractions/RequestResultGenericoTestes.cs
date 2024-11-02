using OnForkHub.Core.Abstractions;
using OnForkHub.Core.Enums;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestResultGenericoTestes
{
    [Fact]
    public void DeveRetornarSucessoComDados()
    {
        // Arrange
        var dados = "Dados de teste";

        // Act
        var resultado = RequestResult<string>.Success(dados);

        // Assert
        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.Data.Should().Be(dados);
    }

    [Fact]
    public void DeveConverterDadosParaResultadoSucesso()
    {
        // Arrange
        string dados = "Dados de teste";

        // Act
        RequestResult<string> resultado = dados;

        // Assert
        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.Data.Should().Be(dados);
    }

    [Fact]
    public void DeveConverterExcecaoParaResultadoErro()
    {
        // Arrange
        var excecao = new Exception("Exceção de teste");

        // Act
        RequestResult<string> resultado = excecao;

        // Assert
        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.Error.Should().NotBeNull();
        resultado.Error!.Description.Should().Be(excecao.Message);
    }

    [Fact]
    public void DeveRetornarResultadoComValidacoes()
    {
        // Arrange
        var validacao = new Validation("PropriedadeTeste", "Mensagem de validação de teste");

        // Act
        var resultado = RequestResult<string>.WithValidations(validacao);

        // Assert
        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().ContainSingle();
        resultado.Validations.First().Should().Be(validacao);
    }

    [Fact]
    public void DeveRetornarResultadoSemConteudo()
    {
        // Act
        var resultado = RequestResult<string>.WithNoContent();

        // Assert
        resultado.Status.Should().Be(ECustomResultStatus.NoContent);
        resultado.Data.Should().BeNull();
    }

    [Fact]
    public void DeveInicializarRequestResultGenericoSemDados()
    {
        // Act
        var resultado = new RequestResult<string>();

        // Assert
        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.Data.Should().BeNull();
        resultado.EntityErrors.Should().NotBeNull().And.BeEmpty();
        resultado.GeneralErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DeveAdicionarErrosDeEntidadeEmRequestResultGenerico()
    {
        var errosDeEntidade = new Dictionary<string, List<string>>
        {
            {
                "EntidadeTeste",
                new List<string> { "Erro de entidade 1", "Erro de entidade 2" }
            },
        };

        var resultado = RequestResult<string>.WithError(errosDeEntidade);

        resultado.Status.Should().Be(ECustomResultStatus.EntityHasError);
        resultado.EntityErrors.Should().ContainKey("EntidadeTeste");
        resultado
            .EntityErrors["EntidadeTeste"]
            .Should()
            .BeEquivalentTo(new List<string> { "Erro de entidade 1", "Erro de entidade 2" });
    }

    [Fact]
    public void DeveRetornarErroComObjetoErroEmRequestResultGenerico()
    {
        var error = new Error("Erro específico de teste");

        var resultado = RequestResult<string>.WithError(error);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.Error.Should().Be(error);
    }

    [Fact]
    public void DeveRetornarSemConteudoEmRequestResultGenerico()
    {
        var resultado = RequestResult<string>.WithNoContent();

        resultado.Status.Should().Be(ECustomResultStatus.NoContent);
        resultado.Data.Should().BeNull();
    }

    [Fact]
    public void DeveAdicionarErroDeValidacaoSemNomeDeCampo()
    {
        var mensagemErro = "Erro de validação genérico";

        var resultado = RequestResult.WithValidationError(mensagemErro);

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.ValidationResult.Errors.Should().ContainSingle();
        resultado.ValidationResult.Errors.First().Message.Should().Be(mensagemErro);
        resultado.ValidationResult.Errors.First().Field.Should().BeEmpty();
    }

    [Fact]
    public void DeveConverterArrayDeValidacoesParaRequestResultGenerico()
    {
        var validacoes = new[]
        {
            new Validation("PropriedadeTeste1", "Erro de validação 1"),
            new Validation("PropriedadeTeste2", "Erro de validação 2"),
        };

        RequestResult<string> resultado = validacoes;

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().BeEquivalentTo(validacoes);
    }

    [Fact]
    public void DeveFormatarMensagensDeErroCorretamenteEmRequestResultGenerico()
    {
        var resultado = new RequestResult<string>();
        resultado.AddError("Erro geral 1");
        resultado.AddEntityError("Entidade1", "Erro de entidade 1");

        var textoFormatado = resultado.ToString();

        textoFormatado.Should().Contain("Erro geral 1");
        textoFormatado.Should().Contain("Entidade1: Erro de entidade 1");
    }

    [Fact]
    public void DeveAdicionarValidacaoComNomeCampoEDescricaoEmRequestResultGenerico()
    {
        var resultado = RequestResult<string>.WithValidations(
            "CampoTeste",
            "Erro de validação teste"
        );

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().ContainSingle();
        resultado.Validations.First().PropertyName.Should().Be("CampoTeste");
        resultado.Validations.First().Description.Should().Be("Erro de validação teste");
    }
}
