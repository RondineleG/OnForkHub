using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Video : BaseEntity
{
    public string Titulo { get; private set; } = null!;
    public string Descricao { get; private set; } = string.Empty;
    public Url Url { get; private set; }
    public long UsuarioId { get; private set; }

    private readonly List<Categoria> _categorias;
    public IReadOnlyCollection<Categoria> Categorias => _categorias.AsReadOnly();

    private Video()
    {
        _categorias = new List<Categoria>();
    }

    public static Video Create(string titulo, string descricao, string url, long usuarioId)
    {
        var video = new Video
        {
            Titulo = titulo,
            Descricao = descricao,
            Url = Url.Create(url),
            UsuarioId = usuarioId
        };

        video.Validate();
        return video;
    }

    public static Video Load(long id, string titulo, string descricao,
        string url, long usuarioId, DateTime createdAt, DateTime? updatedAt = null)
    {
        var video = new Video
        {
            Titulo = titulo,
            Descricao = descricao,
            Url = Url.Create(url),
            UsuarioId = usuarioId
        };

        video.SetId(id, createdAt, updatedAt);
        video.Validate();
        return video;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public override void Validate()
    {
        DomainException.When(string.IsNullOrWhiteSpace(Titulo),
            "Título é obrigatório");
        DomainException.When(Titulo.Length < 3,
            "Título deve ter pelo menos 3 caracteres");
        DomainException.When(Titulo.Length > 200,
            "Título deve ter no máximo 200 caracteres");
        DomainException.When(UsuarioId <= 0,
            "UserId é obrigatório");
    }

    public void AdicionarCategoria(Categoria categoria)
    {
        DomainException.When(categoria == null,
            "Categoria não pode ser nula");

        if (!_categorias.Contains(categoria))
        {
            _categorias.Add(categoria);
            Update();
        }
    }

    public void RemoverCategoria(Categoria categoria)
    {
        DomainException.When(categoria == null,
            "Categoria não pode ser nula");

        if (_categorias.Contains(categoria))
        {
            _categorias.Remove(categoria);
            Update();
        }
    }

    public void AtualizarDados(string titulo, string descricao, string url)
    {
        Titulo = titulo;
        Descricao = descricao;
        Url = Url.Create(url);
        Validate();
        Update();
    }
}

