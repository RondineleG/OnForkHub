using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; private set; } = null!;
    public Email Email { get; private set; }

    private readonly List<Video> _videos;
    public IReadOnlyCollection<Video> Videos => _videos.AsReadOnly();

    private Usuario()
    {
        _videos = new List<Video>();
    }

    public static Usuario Create(string nome, string email)
    {
        var usuario = new Usuario
        {
            Nome = nome,
            Email = Email.Create(email)
        };

        usuario.Validate();
        return usuario;
    }

    public static Usuario Load(long id, string nome, string email,
        DateTime createdAt, DateTime? updatedAt = null)
    {
        var usuario = new Usuario
        {
            Nome = nome,
            Email = Email.Create(email)
        };

        usuario.SetId(id, createdAt, updatedAt);
        usuario.Validate();
        return usuario;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    public override void Validate()
    {
        DomainException.When(string.IsNullOrWhiteSpace(Nome),
            "Nome é obrigatório");
        DomainException.When(Nome.Length < 3,
            "Nome deve ter pelo menos 3 caracteres");
        DomainException.When(Nome.Length > 100,
            "Nome deve ter no máximo 100 caracteres");
    }

    public void AdicionarVideo(Video video)
    {
        DomainException.When(video == null, "Video não pode ser nulo");
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
}

