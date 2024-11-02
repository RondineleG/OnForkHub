namespace OnForkHub.Core.Entities;

public class Categoria : BaseEntity
{
    public string Nome { get; private set; } = null!;

    public string Descricao { get; private set; } = string.Empty;

    private Categoria() { }

    public static Categoria Create(string nome, string descricao)
    {
        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = descricao
        };

        categoria.Validate();
        return categoria;
    }

    public static Categoria Load(long id, string nome, string descricao,
        DateTime createdAt, DateTime? updatedAt = null)
    {
        var categoria = new Categoria
        {
            Nome = nome,
            Descricao = descricao
        };

        categoria.SetId(id, createdAt, updatedAt);
        categoria.Validate();
        return categoria;
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
        DomainException.When(Nome.Length > 50,
            "Nome deve ter no máximo 50 caracteres");
    }

    public void AtualizarDados(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;
        Validate();
        Update();
    }
}

