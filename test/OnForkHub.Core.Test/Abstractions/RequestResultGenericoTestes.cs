using OnForkHub.Core.Abstractions;
using OnForkHub.Core.Enums;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestResultGenericoTestes
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar sucesso com dados fornecidos")]
    public void DeveRetornarSucessoComDados()
    {
        var dados = "Dados de teste";

        var resultado = RequestResult<string>.Success(dados);

        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.Data.Should().Be(dados);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve converter dados para resultado de sucesso")]
    public void DeveConverterDadosParaResultadoSucesso()
    {
        var dados = "Dados de teste";

        RequestResult<string> resultado = dados;

        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.Data.Should().Be(dados);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve converter exceção para resultado de erro")]
    public void DeveConverterExcecaoParaResultadoErro()
    {
        var excecao = new Exception("Exceção de teste");

        RequestResult<string> resultado = excecao;

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.RequestError.Should().NotBeNull();
        resultado.RequestError!.Description.Should().Be(excecao.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado com validações")]
    public void DeveRetornarResultadoComValidacoes()
    {
        var validacao = new RequestValidation("PropriedadeTeste", "Mensagem de validação de teste");

        var resultado = RequestResult<string>.WithValidations(validacao);

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().ContainSingle();
        resultado.Validations.First().Should().Be(validacao);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado sem conteúdo")]
    public void DeveRetornarResultadoSemConteudo()
    {
        var resultado = RequestResult<string>.WithNoContent();

        resultado.Status.Should().Be(ECustomResultStatus.NoContent);
        resultado.Data.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar RequestResult genérico sem dados")]
    public void DeveInicializarRequestResultGenericoSemDados()
    {
        var resultado = new RequestResult<string>();

        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.Data.Should().BeNull();
        resultado.EntityErrors.Should().NotBeNull().And.BeEmpty();
        resultado.GeneralErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erros de entidade em RequestResult genérico")]
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
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro com objeto de erro em RequestResult genérico")]
    public void DeveRetornarErroComObjetoErroEmRequestResultGenerico()
    {
        var error = new RequestError("Erro específico de teste");

        var resultado = RequestResult<string>.WithError(error);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.RequestError.Should().Be(error);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar sem conteúdo em RequestResult genérico")]
    public void DeveRetornarSemConteudoEmRequestResultGenerico()
    {
        var resultado = RequestResult<string>.WithNoContent();

        resultado.Status.Should().Be(ECustomResultStatus.NoContent);
        resultado.Data.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro de validação sem nome de campo")]
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
    [Trait("Category", "Unit")]
    [DisplayName("Deve converter array de validações para RequestResult genérico")]
    public void DeveConverterArrayDeValidacoesParaRequestResultGenerico()
    {
        var validacoes = new[]
        {
            new RequestValidation("PropriedadeTeste1", "Erro de validação 1"),
            new RequestValidation("PropriedadeTeste2", "Erro de validação 2"),
        };

        RequestResult<string> resultado = validacoes;

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().BeEquivalentTo(validacoes);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve formatar mensagens de erro corretamente em RequestResult genérico")]
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
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar validação com nome de campo e descrição em RequestResult genérico")]
    public void DeveAdicionarValidacaoComNomeCampoEDescricaoEmRequestResultGenerico()
    {
        var resultado = RequestResult<string>.WithValidations("CampoTeste", "Erro de validação teste");

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().ContainSingle();
        resultado.Validations.First().PropertyName.Should().Be("CampoTeste");
        resultado.Validations.First().Description.Should().Be("Erro de validação teste");
    }
}
