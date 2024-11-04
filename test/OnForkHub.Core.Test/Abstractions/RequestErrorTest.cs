using System.Text.Json;
using OnForkHub.Core.Abstractions;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestErrorTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar corretamente com descrição")]
    public void DeveInicializarCorretamenteComDescricao()
    {
        var description = "Erro de validação";

        var error = new RequestError(description);

        error.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve ser igual para instâncias com mesma descrição")]
    public void DeveSerIgualParaInstanciasComMesmaDescricao()
    {
        var error1 = new RequestError("Erro de validação");
        var error2 = new RequestError("Erro de validação");

        error1.Should().Be(error2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve ser diferente para instâncias com descrições diferentes")]
    public void DeveSerDiferenteParaInstanciasComDescricoesDiferentes()
    {
        var error1 = new RequestError("Erro de validação");
        var error2 = new RequestError("Outro erro");

        error1.Should().NotBe(error2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve manter imutabilidade ao criar cópia modificada")]
    public void DeveManterImutabilidadeAoCriarCopiaModificada()
    {
        var error = new RequestError("Erro de validação");

        var modifiedError = error with { Description = "Novo erro" };

        modifiedError.Should().NotBeSameAs(error);
        modifiedError.Description.Should().Be("Novo erro");
        error.Description.Should().Be("Erro de validação");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve serializar para JSON corretamente")]
    public void DeveSerializarParaJsonCorretamente()
    {
        var error = new RequestError("Erro de validação");

        var json = JsonSerializer.Serialize(error);

        var deserializedError = JsonSerializer.Deserialize<RequestError>(json);

        deserializedError.Should().NotBeNull();
        deserializedError!.Description.Should().Be("Erro de validação");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve desserializar de JSON corretamente")]
    public void DeveDesserializarDeJsonCorretamente()
    {
        var json = /*lang=json,strict*/
            "{\"Description\":\"Erro de validação\"}";

        var error = JsonSerializer.Deserialize<RequestError>(json);

        error.Should().NotBeNull();
        error!.Description.Should().Be("Erro de validação");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falso ao comparar com null")]
    public void DeveRetornarFalsoAoCompararComNull()
    {
        var error = new RequestError("Erro de validação");

        error.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar string formatada corretamente ao chamar ToString")]
    public void DeveRetornarStringFormatadaCorretamenteAoChamarToString()
    {
        var error = new RequestError("Erro de validação");

        var stringRepresentation = error.ToString();

        stringRepresentation.Should().Contain("Erro de validação");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve gerar hash code igual para instâncias com mesma descrição")]
    public void DeveGerarHashCodeIgualParaInstanciasComMesmaDescricao()
    {
        var error1 = new RequestError("Erro de validação");
        var error2 = new RequestError("Erro de validação");

        error1.GetHashCode().Should().Be(error2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve gerar hash code diferente para instâncias com descrições diferentes")]
    public void DeveGerarHashCodeDiferenteParaInstanciasComDescricoesDiferentes()
    {
        var error1 = new RequestError("Erro de validação");
        var error2 = new RequestError("Outro erro");

        error1.GetHashCode().Should().NotBe(error2.GetHashCode());
    }
}
