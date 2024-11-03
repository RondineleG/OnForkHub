using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Video : BaseEntity
{
    private Video()
    {
        this._categorias = [];
    }

    private readonly List<Categoria> _categorias;

    public IReadOnlyCollection<Categoria> Categorias => this._categorias.AsReadOnly();

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

        if (!this._categorias.Contains(categoria))
        {
            this._categorias.Add(categoria);
            this.Update();
        }
    }

    public void AtualizarDados(string titulo, string descricao, string url)
    {
        this.Titulo = titulo;
        this.Descricao = descricao;
        this.Url = Url.Create(url);
        this.Validate();
        this.Update();
    }

    public void RemoverCategoria(Categoria categoria)
    {
        DomainException.ThrowErrorWhen(() => categoria == null, "Categoria não pode ser nula");

        if (this._categorias.Contains(categoria))
        {
            this._categorias.Remove(categoria);
            this.Update();
        }
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult()
            .AddErrorIfNullOrWhiteSpace(this.Titulo, $"{nameof(this.Titulo)} é obrigatório", nameof(this.Titulo))
            .AddErrorIf(
                this.Titulo.Length < 3,
                $"{nameof(this.Titulo)} deve ter pelo menos 3 caracteres",
                nameof(this.Titulo)
            )
            .AddErrorIf(
                this.Titulo.Length > 50,
                $"{nameof(this.Titulo)} deve ter no máximo 50 caracteres",
                nameof(this.Titulo)
            )
            .AddErrorIfNullOrWhiteSpace(
                this.Descricao,
                $"{nameof(this.Descricao)} é obrigatório",
                nameof(this.Descricao)
            )
            .AddErrorIf(
                this.Descricao.Length < 5,
                $"{nameof(this.Descricao)} deve ter pelo menos 5 caracteres",
                nameof(this.Descricao)
            )
            .AddErrorIf(
                this.Descricao.Length > 200,
                $"{nameof(this.Descricao)} deve ter no máximo 200 caracteres",
                nameof(this.Descricao)
            )
            .AddErrorIf(this.UsuarioId <= 0, $"{nameof(this.Descricao)} é obrigatório", nameof(this.Descricao));

        validationResult.ThrowIfInvalid();

        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        this.Id = id;
        this.CreatedAt = createdAt;
        this.UpdatedAt = updatedAt;
    }
}
