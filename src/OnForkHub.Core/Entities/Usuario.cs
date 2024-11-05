using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;
using OnForkHub.Core.ValueObjects;

namespace OnForkHub.Core.Entities;

public class Usuario : BaseEntity
{
    private Usuario()
    {
        _videos = [];
    }

    private readonly List<Video> _videos;

    public Email Email { get; private set; }

    public Name Name { get; private set; } = null!;

    public IReadOnlyCollection<Video> Videos => _videos.AsReadOnly();

    public static Usuario Create(string name, string email)
    {
        var usuario = new Usuario { Name = Name.Create(name), Email = Email.Create(email) };
        usuario.Validate();
        return usuario;
    }

    public static Usuario Load(long id, string name, string email, DateTime createdAt, DateTime? updatedAt = null)
    {
        var usuario = new Usuario { Name = Name.Create(name), Email = Email.Create(email) };

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

    public void AtualizarDados(string name, string email)
    {
        Name = Name.Create(name);
        Email = Email.Create(email);
        Validate();
        Update();
    }

    public void AtualizarNome(string name)
    {
        Name = Name.Create(name);
        Validate();
        Update();
    }

    public void AtualizarEmail(string email)
    {
        Email = Email.Create(email);
        Validate();
        Update();
    }

    //Validação está sendo feita dentro de ValueObjects
    public override ValidationResult Validate()
    {
        return new ValidationResult();
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
