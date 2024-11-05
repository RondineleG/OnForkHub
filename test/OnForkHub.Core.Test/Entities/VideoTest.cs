namespace OnForkHub.Core.Test.Entities;

public class VideoTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar video com sucesso quando dados válidos")]
    public void DeveCriarVideoComSucessoQuandoDadosValidos()
    {
        var titulo = "Video Teste";
        var descricao = "Descrição do vídeo";
        var url = "https://example.com/video";
        var usuarioId = 1L;

        var video = Video.Create(titulo, descricao, url, usuarioId);

        video.Should().NotBeNull();
        video.Title.Value.Should().Be(titulo);
        video.Descricao.Should().Be(descricao);
        video.Url.Value.Should().Be(url);
        video.UsuarioId.Should().Be(usuarioId);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao validar vídeo com título vazio")]
    public void DeveRetornarErroAoValidarVideoComTituloVazio()
    {
        var titulo = "";
        var descricao = "Descrição do vídeo";
        var url = "https://example.com/video";
        var usuarioId = 1L;

        Action act = () => Video.Create(titulo, descricao, url, usuarioId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve carregar video com sucesso quando dados válidos")]
    public void DeveCarregarVideoComSucessoQuandoDadosValidos()
    {
        var id = 1L;
        var titulo = "Video Teste";
        var descricao = "Descrição do vídeo";
        var url = "https://example.com/video";
        var usuarioId = 1L;
        var createdAt = DateTime.Now;

        var video = Video.Load(id, titulo, descricao, url, usuarioId, createdAt);

        video.Should().NotBeNull();
        video.Id.Should().Be(id);
        video.Title.Value.Should().Be(titulo);
        video.Descricao.Should().Be(descricao);
        video.Url.Value.Should().Be(url);
        video.UsuarioId.Should().Be(usuarioId);
        video.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve atualizar dados do vídeo com sucesso quando dados válidos")]
    public void DeveAtualizarDadosDoVideoComSucessoQuandoDadosValidos()
    {
        var video = Video.Create("Título original", "Descrição original", "https://original.com/video", 1L);
        var novoTitulo = "Novo Título";
        var novaDescricao = "Nova descrição";
        var novaUrl = "https://new.com/video";

        video.AtualizarDados(novoTitulo, novaDescricao, novaUrl);

        video.Title.Value.Should().Be(novoTitulo);
        video.Descricao.Should().Be(novaDescricao);
        video.Url.Value.Should().Be(novaUrl);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao atualizar dados com título inválido")]
    public void DeveRetornarErroAoAtualizarDadosComTituloInvalido()
    {
        var video = Video.Create("Título original", "Descrição original", "https://original.com/video", 1L);
        var novoTitulo = "Ti";
        var novaDescricao = "Nova descrição";
        var novaUrl = "https://new.com/video";

        Action act = () => video.AtualizarDados(novoTitulo, novaDescricao, novaUrl);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve adicionar categoria ao vídeo")]
    public void DeveAdicionarCategoriaAoVideo()
    {
        var video = Video.Create("Título", "Descrição", "https://example.com/video", 1L);
        var categoria = Categoria.Create("Categoria", "Descrição da categoria").Data!;

        video.AdicionarCategoria(categoria);

        video.Categorias.Should().Contain(categoria);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao adicionar categoria nula")]
    public void DeveRetornarErroAoAdicionarCategoriaNula()
    {
        var video = Video.Create("Título", "Descrição", "https://example.com/video", 1L);

        Action act = () => video.AdicionarCategoria(null);

        act.Should().Throw<DomainException>().WithMessage("Categoria não pode ser nula");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve remover categoria do vídeo")]
    public void DeveRemoverCategoriaDoVideo()
    {
        var video = Video.Create("Título", "Descrição", "https://example.com/video", 1L);
        var categoria = Categoria.Create("Categoria", "Descrição da categoria").Data!;
        video.AdicionarCategoria(categoria);

        video.RemoverCategoria(categoria);

        video.Categorias.Should().NotContain(categoria);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro ao remover categoria nula")]
    public void DeveRetornarErroAoRemoverCategoriaNula()
    {
        var video = Video.Create("Título", "Descrição", "https://example.com/video", 1L);

        Action act = () => video.RemoverCategoria(null);

        act.Should().Throw<DomainException>().WithMessage("Categoria não pode ser nula");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve validar vídeo corretamente")]
    public void DeveValidarVideoCorretamente()
    {
        var video = Video.Create("Título válido", "Descrição válida", "https://example.com/video", 1L);

        var validationResult = video.Validate();

        validationResult.Errors.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar erro de validação para título com mais de 50 caracteres")]
    public void DeveRetornarErroDeValidacaoParaTituloComMaisDe50Caracteres()
    {
        var titulo = new string('A', 51);
        var descricao = "Descrição válida";
        var url = "https://example.com/video";
        var usuarioId = 1L;

        Action act = () => Video.Create(titulo, descricao, url, usuarioId);

        act.Should().Throw<DomainException>();
    }
}
