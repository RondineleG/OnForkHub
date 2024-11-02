using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Video : BaseEntity
{
    private Video()
    {
        _categorias = new List<Categoria>();
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

    public void AtualizarDados(string titulo, string descricao, string url)
    {
        Titulo = titulo;
        Descricao = descricao;
        Url = Url.Create(url);
        Validate();
        Update();
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
        var validationResult = new ValidationResult()
            // Titulo
            .AddErrorIfNullOrWhiteSpace(Titulo, $"{nameof(Titulo)} é obrigatório", nameof(Titulo))
            .AddErrorIf(
                Titulo.Length < 3,
                $"{nameof(Titulo)} deve ter pelo menos 3 caracteres",
                nameof(Titulo)
            )
            .AddErrorIf(
                Titulo.Length > 50,
                $"{nameof(Titulo)} deve ter no máximo 50 caracteres",
                nameof(Titulo)
            )
            //Descricao
            .AddErrorIfNullOrWhiteSpace(
                Descricao,
                $"{nameof(Descricao)} é obrigatório",
                nameof(Descricao)
            )
            .AddErrorIf(
                Descricao.Length < 5,
                $"{nameof(Descricao)} deve ter pelo menos 5 caracteres",
                nameof(Descricao)
            )
            .AddErrorIf(
                Descricao.Length > 200,
                $"{nameof(Descricao)} deve ter no máximo 200 caracteres",
                nameof(Descricao)
            )
            //UsuarioId
            .AddErrorIf(UsuarioId <= 0, $"{nameof(Descricao)} é obrigatório", nameof(Descricao));

        validationResult.ThrowIfInvalid();

        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
