using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Usuario : BaseEntity
{
    private Usuario()
    {
        this._videos = [];
    }

    private readonly List<Video> _videos;

    public Email Email { get; private set; }

    public string Nome { get; private set; } = null!;

    public IReadOnlyCollection<Video> Videos => this._videos.AsReadOnly();

    public static Usuario Create(string nome, string email)
    {
        var usuario = new Usuario { Nome = nome, Email = Email.Create(email) };

        usuario.Validate();
        return usuario;
    }

    public static Usuario Load(long id, string nome, string email, DateTime createdAt, DateTime? updatedAt = null)
    {
        var usuario = new Usuario { Nome = nome, Email = Email.Create(email) };

        usuario.SetId(id, createdAt, updatedAt);
        usuario.Validate();
        return usuario;
    }

    public void AdicionarVideo(Video video)
    {
        DomainException.ThrowErrorWhen(() => video == null, "Video não pode ser nulo");
        this._videos.Add(video);
        this.Update();
    }

    public void AtualizarDados(string nome, string email)
    {
        this.Nome = nome;
        this.Email = Email.Create(email);
        this.Validate();
        this.Update();
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(this.Nome, "Nome é obrigatório", "Nome");
        validationResult.AddErrorIf(this.Nome.Length < 3, "Nome deve ter pelo menos 3 caracteres", "Nome");
        validationResult.AddErrorIf(this.Nome.Length > 50, "Nome deve ter no máximo 50 caracteres", "Nome");
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
