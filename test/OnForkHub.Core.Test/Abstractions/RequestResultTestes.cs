using OnForkHub.Core.Abstractions;
using OnForkHub.Core.Enums;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestResultTestes
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar com status de sucesso")]
    public void DeveInicializarComStatusSucesso()
    {
        var resultado = new RequestResult();

        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.ValidationResult.Should().NotBeNull();
        resultado.EntityErrors.Should().NotBeNull().And.BeEmpty();
        resultado.GeneralErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado de erro ao informar mensagem")]
    public void DeveRetornarResultadoErroQuandoInformadoMensagem()
    {
        var mensagemErro = "Mensagem de erro de teste";

        var resultado = RequestResult.WithError(mensagemErro);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.RequestError.Should().NotBeNull();
        resultado.RequestError!.Description.Should().Be(mensagemErro);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado de erro ao informar exceção")]
    public void DeveRetornarResultadoErroQuandoInformadoExcecao()
    {
        var excecao = new Exception("Exceção de teste");

        var resultado = RequestResult.WithError(excecao);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.RequestError.Should().NotBeNull();
        resultado.RequestError!.Description.Should().Be(excecao.Message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado de erro ao informar lista de erros")]
    public void DeveRetornarResultadoErroQuandoInformadoListaErros()
    {
        var erros = new List<string> { "Erro 1", "Erro 2" };

        var resultado = RequestResult.WithError(erros);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.GeneralErrors.Should().BeEquivalentTo(erros);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro de entidade")]
    public void DeveAdicionarErroDeEntidade()
    {
        var resultado = new RequestResult();
        var entidade = "EntidadeTeste";
        var mensagem = "Mensagem de erro de teste";

        resultado.AddEntityError(entidade, mensagem);

        resultado.Status.Should().Be(ECustomResultStatus.EntityHasError);
        resultado.EntityErrors.Should().ContainKey(entidade);
        resultado.EntityErrors[entidade].Should().Contain(mensagem);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado de entidade não encontrada")]
    public void DeveRetornarResultadoEntidadeNaoEncontrada()
    {
        var entidade = "EntidadeTeste";
        var id = 1;
        var message = "Entidade não encontrada";

        var resultado = RequestResult.EntityNotFound(entidade, id, message);

        resultado.Status.Should().Be(ECustomResultStatus.EntityNotFound);
        resultado.EntityWarning.Should().NotBeNull();
        resultado.EntityWarning!.Name.Should().Be(entidade);
        resultado.EntityWarning.Id.Should().Be(id);
        resultado.EntityWarning.Message.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado de erro de validação")]
    public void DeveRetornarResultadoErroValidacao()
    {
        var mensagemErro = "Erro de validação";
        var nomeCampo = "CampoTeste";

        var resultado = RequestResult.WithValidationError(mensagemErro, nomeCampo);

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.ValidationResult.Errors.Should().ContainSingle();
        resultado.ValidationResult.Errors.First().Message.Should().Be(mensagemErro);
        resultado.ValidationResult.Errors.First().Field.Should().Be(nomeCampo);
    }

    private static readonly string[] expected = ["Erro geral 1", "Erro geral 2"];

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar múltiplos erros gerais")]
    public void DeveAdicionarMultiplosErrosGerais()
    {
        var resultado = new RequestResult();
        resultado.AddError("Erro geral 1");
        resultado.AddError("Erro geral 2");

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.GeneralErrors.Should().HaveCount(2);
        resultado.GeneralErrors.Should().Contain(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar validações como coleção")]
    public void DeveAdicionarValidacoesComoColecao()
    {
        var validacoes = new List<RequestValidation>
        {
            new("Campo1", "Erro de validação 1"),
            new("Campo2", "Erro de validação 2"),
        };

        var resultado = RequestResult.WithValidations(validacoes.ToArray());

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.ValidationResult.Errors.Should().HaveCount(2);
        resultado
            .ValidationResult.Errors.Should()
            .Contain(error => error.Message == "Erro de validação 1" && error.Field == "Campo1");
        resultado
            .ValidationResult.Errors.Should()
            .Contain(error => error.Message == "Erro de validação 2" && error.Field == "Campo2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve formatar mensagens de erro corretamente")]
    public void DeveFormatarMensagensDeErroCorretamente()
    {
        var resultado = new RequestResult();
        resultado.AddError("Erro geral 1");
        resultado.AddError("Erro geral 2");
        resultado.AddEntityError("Entidade1", "Erro de entidade 1");
        resultado.AddEntityError("Entidade1", "Erro de entidade 2");

        var textoFormatado = resultado.ToString();

        textoFormatado.Should().Contain("Erro geral 1");
        textoFormatado.Should().Contain("Erro geral 2");
        textoFormatado.Should().Contain("Entidade1: Erro de entidade 1");
        textoFormatado.Should().Contain("Entidade1: Erro de entidade 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar erro com lista de erros e exceção")]
    public void DeveAdicionarErroComListaEExcecao()
    {
        var erros = new List<string> { "Erro 1", "Erro 2" };
        var excecao = new Exception("Erro de exceção");

        var resultado = RequestResult.WithError(erros);
        resultado.AddError(excecao.Message);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.GeneralErrors.Should().Contain(new[] { "Erro 1", "Erro 2", excecao.Message });
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar campos de erro apenas quando acessados")]
    public void DeveInicializarCamposDeErroSomenteQuandoAcessados()
    {
        var resultado = new RequestResult();

        resultado.EntityErrors.Should().BeEmpty();
        resultado.GeneralErrors.Should().BeEmpty();

        resultado.EntityErrors.Should().NotBeNull();
        resultado.GeneralErrors.Should().NotBeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve formatar mensagens de validação no método ToString")]
    public void DeveFormatarMensagensDeValidacaoNoToString()
    {
        var resultado = RequestResult.WithValidations(
            new RequestValidation("CampoTeste", "Erro de validação 1"),
            new RequestValidation("CampoTeste", "Erro de validação 2")
        );

        var textoFormatado = resultado.ToString();

        textoFormatado.Should().Contain("CampoTeste: Erro de validação 1");
        textoFormatado.Should().Contain("CampoTeste: Erro de validação 2");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar múltiplos erros para a mesma entidade")]
    public void DeveAdicionarMultiplosErrosParaMesmaEntidade()
    {
        string[] expected = ["Erro de entidade 1", "Erro de entidade 2"];
        var resultado = new RequestResult();
        var entidade = "EntidadeTeste";

        resultado.AddEntityError(entidade, "Erro de entidade 1");
        resultado.AddEntityError(entidade, "Erro de entidade 2");

        resultado.Status.Should().Be(ECustomResultStatus.EntityHasError);
        resultado.EntityErrors.Should().ContainKey(entidade);
        resultado.EntityErrors[entidade].Should().Contain(expected);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar resultado sem conteúdo")]
    public void DeveRetornarResultadoSemConteudo()
    {
        var resultado = RequestResult.WithNoContent();

        resultado.Status.Should().Be(ECustomResultStatus.NoContent);
        resultado.EntityErrors.Should().BeEmpty();
        resultado.GeneralErrors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar validação com propriedade e descrição")]
    public void DeveAdicionarValidacaoComPropriedadeEDescricao()
    {
        var resultado = RequestResult.WithValidations(
            new RequestValidation("PropriedadeTeste", "Erro de validação de teste")
        );

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);

        resultado.ValidationResult.Errors.Should().ContainSingle();

        var erro = resultado.ValidationResult.Errors.First();
        erro.Field.Should().Be("PropriedadeTeste");
        erro.Message.Should().Be("Erro de validação de teste");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro com objeto de erro personalizado")]
    public void DeveRetornarErroComObjetoErroPersonalizado()
    {
        var erroPersonalizado = new RequestError("Erro personalizado");

        var resultado = RequestResult.WithError(erroPersonalizado);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.RequestError.Should().Be(erroPersonalizado);
    }
}
