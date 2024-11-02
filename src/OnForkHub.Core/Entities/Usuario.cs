using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Usuario : BaseEntity
{
    private Usuario()
    {
        _videos = new List<Video>();
    }

    private readonly List<Video> _videos;

    public Email Email { get; private set; }

    public string Nome { get; private set; } = null!;

    public IReadOnlyCollection<Video> Videos => _videos.AsReadOnly();

    public static Usuario Create(string nome, string email)
    {
        var usuario = new Usuario { Nome = nome, Email = Email.Create(email) };

        usuario.Validate();
        return usuario;
    }

    public static Usuario Load(
        long id,
        string nome,
        string email,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        var usuario = new Usuario { Nome = nome, Email = Email.Create(email) };

        usuario.SetId(id, createdAt, updatedAt);
        usuario.Validate();
        return usuario;
    }

    public void AdicionarVideo(Video video)
    {
        DomainException.ThrowErrorWhen(() => video == null, "Video não pode ser nulo");
        _videos.Add(video);
        Update();
    }

    public void AtualizarDados(string nome, string email)
    {
        Nome = nome;
        Email = Email.Create(email);
        Validate();
        Update();
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Nome, "Nome é obrigatório", "Nome");
        validationResult.AddErrorIf(
            Nome.Length < 3,
            "Nome deve ter pelo menos 3 caracteres",
            "Nome"
        );
        validationResult.AddErrorIf(
            Nome.Length > 50,
            "Nome deve ter no máximo 50 caracteres",
            "Nome"
        );
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
