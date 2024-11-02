using OnForkHub.Core.Abstractions;
using OnForkHub.Core.Enums;
using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestResultTestes
{
    [Fact]
    public void DeveInicializarComStatusSucesso()
    {
        var resultado = new RequestResult();

        resultado.Status.Should().Be(ECustomResultStatus.Success);
        resultado.ValidationResult.Should().NotBeNull();
        resultado.EntityErrors.Should().NotBeNull().And.BeEmpty();
        resultado.GeneralErrors.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void DeveRetornarResultadoErroQuandoInformadoMensagem()
    {
        var mensagemErro = "Mensagem de erro de teste";

        var resultado = RequestResult.WithError(mensagemErro);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.Error.Should().NotBeNull();
        resultado.Error!.Description.Should().Be(mensagemErro);
    }

    [Fact]
    public void DeveRetornarResultadoErroQuandoInformadoExcecao()
    {
        var excecao = new Exception("Exceção de teste");

        var resultado = RequestResult.WithError(excecao);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.Error.Should().NotBeNull();
        resultado.Error!.Description.Should().Be(excecao.Message);
    }

    [Fact]
    public void DeveRetornarResultadoErroQuandoInformadoListaErros()
    {
        var erros = new List<string> { "Erro 1", "Erro 2" };

        var resultado = RequestResult.WithError(erros);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.GeneralErrors.Should().BeEquivalentTo(erros);
    }

    [Fact]
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

    [Fact]
    public void DeveAdicionarMultiplosErrosGerais()
    {
        var resultado = new RequestResult();
        resultado.AddError("Erro geral 1");
        resultado.AddError("Erro geral 2");

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.GeneralErrors.Should().HaveCount(2);
        resultado.GeneralErrors.Should().Contain(new[] { "Erro geral 1", "Erro geral 2" });
    }

    [Fact]
    public void DeveAdicionarValidacoesComoColecao()
    {
        var validacoes = new List<Validation>
        {
            new Validation("Campo1", "Erro de validação 1"),
            new Validation("Campo2", "Erro de validação 2"),
        };

        var resultado = RequestResult.WithValidations(validacoes);

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().HaveCount(2);
        resultado.Validations.Should().Contain(validacoes);
    }

    [Fact]
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
    public void DeveInicializarCamposDeErroSomenteQuandoAcessados()
    {
        var resultado = new RequestResult();

        resultado.EntityErrors.Should().BeEmpty();
        resultado.GeneralErrors.Should().BeEmpty();

        resultado.EntityErrors.Should().NotBeNull();
        resultado.GeneralErrors.Should().NotBeNull();
    }

    [Fact]
    public void DeveFormatarMensagensDeValidacaoNoToString()
    {
        var resultado = RequestResult.WithValidations(
            new Validation("CampoTeste", "Erro de validação 1"),
            new Validation("CampoTeste", "Erro de validação 2")
        );

        var textoFormatado = resultado.ToString();

        textoFormatado.Should().Contain("CampoTeste: Erro de validação 1");
        textoFormatado.Should().Contain("CampoTeste: Erro de validação 2");
    }

    [Fact]
    public void DeveAdicionarMultiplosErrosParaMesmaEntidade()
    {
        var resultado = new RequestResult();
        var entidade = "EntidadeTeste";

        resultado.AddEntityError(entidade, "Erro de entidade 1");
        resultado.AddEntityError(entidade, "Erro de entidade 2");

        resultado.Status.Should().Be(ECustomResultStatus.EntityHasError);
        resultado.EntityErrors.Should().ContainKey(entidade);
        resultado
            .EntityErrors[entidade]
            .Should()
            .Contain(new[] { "Erro de entidade 1", "Erro de entidade 2" });
    }

    [Fact]
    public void DeveRetornarResultadoSemConteudo()
    {
        var resultado = RequestResult.WithNoContent();

        resultado.Status.Should().Be(ECustomResultStatus.NoContent);
        resultado.EntityErrors.Should().BeEmpty();
        resultado.GeneralErrors.Should().BeEmpty();
    }

    [Fact]
    public void DeveAdicionarValidacaoComPropriedadeEDescricao()
    {
        var resultado = RequestResult.WithValidations(
            "PropriedadeTeste",
            "Erro de validação de teste"
        );

        resultado.Status.Should().Be(ECustomResultStatus.HasValidation);
        resultado.Validations.Should().ContainSingle();
        resultado.Validations.First().PropertyName.Should().Be("PropriedadeTeste");
        resultado.Validations.First().Description.Should().Be("Erro de validação de teste");
    }

    [Fact]
    public void DeveRetornarErroComObjetoErroPersonalizado()
    {
        var erroPersonalizado = new Error("Erro personalizado");

        var resultado = RequestResult.WithError(erroPersonalizado);

        resultado.Status.Should().Be(ECustomResultStatus.HasError);
        resultado.Error.Should().Be(erroPersonalizado);
    }
}
