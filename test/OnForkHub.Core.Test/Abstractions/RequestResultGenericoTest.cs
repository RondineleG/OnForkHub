using OnForkHub.Core.Abstractions;
using OnForkHub.Core.Enums;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestResultGenericoTest
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

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status de erro e mensagens de erro")]
    public void DeveRetornarRequestResultComStatusDeErroEMensagensDeErro()
    {
        var generalErrors = new List<string> { "Erro 1", "Erro 2" };

        var result = RequestResult<string>.WithError(generalErrors);

        result.Status.Should().Be(ECustomResultStatus.HasError);
        result.GeneralErrors.Should().BeEquivalentTo(generalErrors);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com lista vazia de erros gerais")]
    public void DeveRetornarRequestResultComListaVaziaDeErrosGerais()
    {
        var generalErrors = new List<string>();

        var result = RequestResult<string>.WithError(generalErrors);

        result.Status.Should().Be(ECustomResultStatus.HasError);
        result.GeneralErrors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve permitir lista de erros gerais com um único erro")]
    public void DevePermitirListaDeErrosGeraisComUmUnicoErro()
    {
        var generalErrors = new List<string> { "Erro único" };

        var result = RequestResult<string>.WithError(generalErrors);

        result.Status.Should().Be(ECustomResultStatus.HasError);
        result.GeneralErrors.Should().ContainSingle().Which.Should().Be("Erro único");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve permitir lista de erros nula e inicializar como lista vazia")]
    public void DevePermitirListaDeErrosNulaEInicializarComoListaVazia()
    {
        var generalErrors = (List<string>)null;
        var result = RequestResult<string>.WithError(generalErrors);

        result.Status.Should().Be(ECustomResultStatus.HasError);
        result.GeneralErrors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status de validação ao usar operador implícito")]
    public void DeveRetornarRequestResultComStatusDeValidacaoAoUsarOperadorImplicito()
    {
        var validation = new RequestValidation("Nome", "Nome é obrigatório");

        RequestResult<string> result = validation;

        result.Status.Should().Be(ECustomResultStatus.HasValidation);
        result.Validations.Should().ContainSingle().Which.Should().Be(validation);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve conter a validação correta na lista de validações ao usar operador implícito")]
    public void DeveConterValidacaoCorretaNaListaDeValidacoesAoUsarOperadorImplicito()
    {
        var validation = new RequestValidation("CampoTeste", "Erro de validação para campo teste");

        RequestResult<string> result = validation;

        result
            .Validations.Should()
            .ContainSingle(v =>
                v.PropertyName == "CampoTeste" && v.Description == "Erro de validação para campo teste"
            );
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar lista de validações com uma única entrada ao usar operador implícito")]
    public void DeveInicializarListaDeValidacoesComUnicaEntradaAoUsarOperadorImplicito()
    {
        var validation = new RequestValidation("Propriedade", "Mensagem de erro");

        RequestResult<string> result = validation;

        result.Validations.Should().HaveCount(1);
        result.Validations.First().PropertyName.Should().Be("Propriedade");
        result.Validations.First().Description.Should().Be("Mensagem de erro");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityAlreadyExists e EntityWarning configurado")]
    public void DeveRetornarRequestResultComStatusEntityAlreadyExistsEEntityWarningConfigurado()
    {
        var entity = "Produto";
        var id = 1;
        var description = "Produto já existe";

        var result = RequestResult<string>.EntityAlreadyExists(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityAlreadyExists);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityHasError e EntityWarning configurado")]
    public void DeveRetornarRequestResultComStatusEntityHasErrorEEntityWarningConfigurado()
    {
        var entity = "Produto";
        var id = 2;
        var description = "Erro no produto";

        var result = RequestResult<string>.EntityHasError(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityHasError);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityNotFound e EntityWarning configurado")]
    public void DeveRetornarRequestResultComStatusEntityNotFoundEEntityWarningConfigurado()
    {
        var entity = "Produto";
        var id = 3;
        var description = "Produto não encontrado";

        var result = RequestResult<string>.EntityNotFound(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityNotFound);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityAlreadyExists e EntityWarning com entidade vazia")]
    public void DeveRetornarRequestResultComEntityAlreadyExistsEEntityWarningComEntidadeVazia()
    {
        var entity = "";
        var id = 1;
        var description = "Produto já existe";

        var result = RequestResult<string>.EntityAlreadyExists(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityAlreadyExists);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityHasError e EntityWarning com descrição nula")]
    public void DeveRetornarRequestResultComEntityHasErrorEEntityWarningComDescricaoNula()
    {
        var entity = "Produto";
        var id = 2;
        string description = null;

        var result = RequestResult<string>.EntityHasError(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityHasError);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityNotFound e EntityWarning com ID de tipo string")]
    public void DeveRetornarRequestResultComEntityNotFoundEEntityWarningComIdTipoString()
    {
        var entity = "Produto";
        var id = "ID-123";
        var description = "Produto não encontrado";

        var result = RequestResult<string>.EntityNotFound(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityNotFound);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityAlreadyExists e EntityWarning com descrição vazia")]
    public void DeveRetornarRequestResultComEntityAlreadyExistsEEntityWarningComDescricaoVazia()
    {
        var entity = "Produto";
        var id = 1;
        var description = "";

        var result = RequestResult<string>.EntityAlreadyExists(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityAlreadyExists);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().Be(id);
        result.EntityWarning.Message.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar RequestResult com status EntityNotFound e EntityWarning com ID nulo")]
    public void DeveRetornarRequestResultComEntityNotFoundEEntityWarningComIdNulo()
    {
        var entity = "Produto";
        object id = null;
        var description = "Produto não encontrado";

        var result = RequestResult<string>.EntityNotFound(entity, id, description);

        result.Status.Should().Be(ECustomResultStatus.EntityNotFound);
        result.EntityWarning.Should().NotBeNull();
        result.EntityWarning!.Name.Should().Be(entity);
        result.EntityWarning.Id.Should().BeNull();
        result.EntityWarning.Message.Should().Be(description);
    }
}
