
using OnForkHub.Core.Validations;

namespace OnForkHub.Core.Entities.Base;
// Exemplo de uso em uma entidade
public class Usuario : BaseEntity
{
    public string Nome { get; private set; }
    public string Email { get; private set; }
    public decimal Salario { get; private set; }

    // Exemplo 1: Validação retornando ValidationResult
    public ValidationResult Validates()
    {
        return ValidarNome() & ValidarEmail() & ValidarSalario();
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

    // Exemplo 2: Validação com exceção
    public void ValidateAndThrow()
    {
        Validates().ThrowIfInvalid();
    }

    // Exemplo 3: Validação em método de negócio
    public ValidationResult AumentarSalario(decimal percentual)
    {
        var validacao = ValidarPercentualAumento(percentual);
        if (validacao.HasError)
            return validacao;

        Salario *= 1 + (percentual / 100);
        return ValidationResult.Success();
    }

    // Exemplo 4: Método que exige validação com exceção
    public void AtualizarDados(string nome, string email)
    {
        // Validação que lança exceção se falhar
        DomainException.When(string.IsNullOrWhiteSpace(nome), "Nome é obrigatório");
        DomainException.When(string.IsNullOrWhiteSpace(email), "Email é obrigatório");

        Nome = nome;
        Email = email;
    }

    // Validações individuais retornando ValidationResult
    private ValidationResult ValidarNome()
    {
        return ValidationResult.Validate(() => string.IsNullOrWhiteSpace(Nome), "Nome é obrigatório")
             & ValidationResult.Validate(() => Nome?.Length < 3, "Nome deve ter pelo menos 3 caracteres")
             & ValidationResult.Validate(() => Nome?.Length > 50, "Nome deve ter no máximo 50 caracteres");
    }

    private ValidationResult ValidarEmail()
    {
        return ValidationResult.Validate(() => string.IsNullOrWhiteSpace(Email), "Email é obrigatório")
             & ValidationResult.Validate(() => !Email?.Contains("@") ?? false, "Email inválido");
    }

    private ValidationResult ValidarSalario()
    {
        return ValidationResult.Validate(() => Salario < 0, "Salário não pode ser negativo");
    }

    private ValidationResult ValidarPercentualAumento(decimal percentual)
    {
        return ValidationResult.Validate(() => percentual <= 0, "Percentual deve ser maior que zero")
             & ValidationResult.Validate(() => percentual > 100, "Percentual não pode ser maior que 100%");
    }
}