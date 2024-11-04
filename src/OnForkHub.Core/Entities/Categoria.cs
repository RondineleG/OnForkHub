using OnForkHub.Core.Abstractions;
using OnForkHub.Core.Entities.Base;
using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Entities;

public class Categoria : BaseEntity
{
    private Categoria() { }

    public string Descricao { get; private set; } = string.Empty;

    public string Nome { get; private set; } = null!;

    public static RequestResult<Categoria> Create(string nome, string descricao)
    {
        var categoria = new Categoria { Nome = nome, Descricao = descricao };

        var validationResult = categoria.Validate();
        return validationResult.Errors.Count > 0
            ? RequestResult<Categoria>.WithError(validationResult.ErrorMessage)
            : RequestResult<Categoria>.Success(categoria);
    }

    public static RequestResult<Categoria> Load(
        long id,
        string nome,
        string descricao,
        DateTime createdAt,
        DateTime? updatedAt = null
    )
    {
        var categoria = new Categoria { Nome = nome, Descricao = descricao };

        var validationResult = categoria.Validate();
        if (validationResult.Errors.Count > 0)
        {
            return RequestResult<Categoria>.WithError(validationResult.ErrorMessage);
        }

        categoria.SetId(id, createdAt, updatedAt);
        return RequestResult<Categoria>.Success(categoria);
    }

    public RequestResult AtualizarDados(string nome, string descricao)
    {
        Nome = nome;
        Descricao = descricao;

        var validationResult = Validate();
        if (validationResult.Errors.Count > 0)
        {
            return RequestResult.WithError(validationResult.ErrorMessage);
        }

        Update();
        return RequestResult.Success();
    }

    public override ValidationResult Validate()
    {
        var validationResult = new ValidationResult();
        validationResult.AddErrorIfNullOrWhiteSpace(Nome, "Nome é obrigatório", "Nome");
        validationResult.AddErrorIf(Nome.Length < 3, "Nome deve ter pelo menos 3 caracteres", "Nome");
        validationResult.AddErrorIf(Nome.Length > 50, "Nome deve ter no máximo 50 caracteres", "Nome");
        return validationResult;
    }

    private void SetId(long id, DateTime createdAt, DateTime? updatedAt)
    {
        DomainException.ThrowErrorWhen(() => id <= 0, "Id deve ser maior que zero");
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}
