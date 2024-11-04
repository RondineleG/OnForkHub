namespace OnForkHub.Core.Test.Entities;

public class CategoriaTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar categoria com sucesso quando dados válidos")]
    public void DeveCriarCategoriaComSucessoQuandoDadosValidos()
    {
        var nome = "Categoria Teste";
        var descricao = "Descrição da categoria";

        var result = Categoria.Create(nome, descricao);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Nome.Should().Be(nome);
        result.Data.Descricao.Should().Be(descricao);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao criar categoria com nome vazio")]
    public void DeveRetornarErroAoCriarCategoriaComNomeVazio()
    {
        var nome = "";
        var descricao = "Descrição da categoria";

        var result = Categoria.Create(nome, descricao);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError.Should().NotBeNull();
        result.RequestError!.Description.Should().Contain("Nome é obrigatório");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve carregar categoria com sucesso quando dados válidos")]
    public void DeveCarregarCategoriaComSucessoQuandoDadosValidos()
    {
        var id = 1L;
        var nome = "Categoria Teste";
        var descricao = "Descrição da categoria";
        var createdAt = DateTime.Now;

        var result = Categoria.Load(id, nome, descricao, createdAt);

        result.Status.Should().Be(EResultStatus.Success);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
        result.Data.Nome.Should().Be(nome);
        result.Data.Descricao.Should().Be(descricao);
        result.Data.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao carregar categoria com ID inválido")]
    public void DeveRetornarErroAoCarregarCategoriaComIdInvalido()
    {
        var id = 0L;
        var nome = "Categoria Teste";
        var descricao = "Descrição da categoria";
        var createdAt = DateTime.Now;

        Action act = () => Categoria.Load(id, nome, descricao, createdAt);
        act.Should().Throw<DomainException>().WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve atualizar dados da categoria com sucesso quando dados válidos")]
    public void DeveAtualizarDadosDaCategoriaComSucessoQuandoDadosValidos()
    {
        var categoria = Categoria.Create("Categoria Original", "Descrição original").Data!;
        var novoNome = "Categoria Atualizada";
        var novaDescricao = "Descrição atualizada";

        var result = categoria.AtualizarDados(novoNome, novaDescricao);

        result.Status.Should().Be(EResultStatus.Success);
        categoria.Nome.Should().Be(novoNome);
        categoria.Descricao.Should().Be(novaDescricao);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao validar nome com mais de 50 caracteres")]
    public void DeveRetornarErroAoValidarNomeComMaisDe50Caracteres()
    {
        var nome = new string('A', 51);
        var descricao = "Descrição válida";

        var result = Categoria.Create(nome, descricao);

        result.Status.Should().Be(EResultStatus.HasError);
        result.RequestError!.Description.Should().Contain("Nome deve ter no máximo 50 caracteres");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve validar categoria corretamente")]
    public void DeveValidarCategoriaCorretamente()
    {
        var categoria = Categoria.Create("Categoria Válida", "Descrição válida").Data!;

        var validationResult = categoria.Validate();

        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro de validação para nome nulo ou vazio")]
    public void DeveRetornarErroDeValidacaoParaNomeNuloOuVazio()
    {
        var categoria = Categoria.Create("Categoria Inicial", "Descrição inicial").Data!;

        categoria.AtualizarDados("", "Nova descrição");

        var validationResult = categoria.Validate();
        validationResult.Errors.Should().ContainSingle(error => error.Message == "Nome é obrigatório");
    }
}
