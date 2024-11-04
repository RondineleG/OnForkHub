using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Video : BaseEntity
{
    private Video()
    {
        _categorias = [];
    }

    private readonly List<Categoria> _categorias;

    public IReadOnlyCollection<Categoria> Categorias => _categorias.AsReadOnly();

    public string Descricao { get; private set; } = string.Empty;

    public string Titulo { get; private set; } = null!;

    public Url Url { get; private set; }

    public long UsuarioId { get; private set; }

    public static Video Create(string titulo, string descricao, string url, long usuarioId)
    {
        var video = new Video
        {
            Titulo = titulo,
            Descricao = descricao,
            Url = Url.Create(url),
            UsuarioId = usuarioId,
        };

        video.Validate();
        return video;
    }

    public static Video Load(
        long id,
        string titulo,
        string descricao,
        string url,
        long usuarioId,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        var video = new Video
        {
            Titulo = titulo,
            Descricao = descricao,
            Url = Url.Create(url),
            UsuarioId = usuarioId,
        };

        video.SetId(id, createdAt, updatedAt);
        video.Validate();
        return video;
    }

    public void AdicionarCategoria(Categoria categoria)
    {
        DomainException.ThrowErrorWhen(() => categoria == null, "Categoria não pode ser nula");

        if (!_categorias.Contains(categoria))
        {
            _categorias.Add(categoria);
            Update();
        }
    }

    public ValidationResult AtualizarDados(string titulo, string descricao, string url)
    {
        Titulo = titulo;
        Descricao = descricao;
        Url = Url.Create(url);

        var validationResult = Validate();
        if (validationResult.Errors.Count == 0)
        {
            Update();
        }
        return validationResult;
    }

    public void RemoverCategoria(Categoria categoria)
    {
        DomainException.ThrowErrorWhen(() => categoria == null, "Categoria não pode ser nula");

        if (_categorias.Contains(categoria))
        {
            _categorias.Remove(categoria);
            Update();
        }
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Titulo, "Titulo é obrigatório", "Titulo");
        validationResult.AddErrorIf(Titulo.Length < 3, "Titulo deve ter pelo menos 3 caracteres", "Titulo");
        validationResult.AddErrorIf(Titulo.Length > 50, "Titulo deve ter no máximo 50 caracteres", "Titulo");
        validationResult.AddErrorIfNullOrWhiteSpace(Descricao, "Descricao é obrigatória", "Descricao");
        validationResult.AddErrorIf(Descricao.Length < 5, "Descricao deve ter pelo menos 5 caracteres", "Descricao");
        validationResult.AddErrorIf(Descricao.Length > 200, "Descricao deve ter no máximo 200 caracteres", "Descricao");
        validationResult.AddErrorIf(UsuarioId <= 0, "UsuarioId é obrigatório", "UsuarioId");

        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
