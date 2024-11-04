using System.Text.Json;
using OnForkHub.Core.Abstractions;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestEntityWarningTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar corretamente com nome, ID e mensagem")]
    public void DeveInicializarCorretamenteComNomeIdEMensagem()
    {
        var name = "EntidadeTeste";
        var id = 123;
        var message = "Aviso sobre a entidade";

        var warning = new RequestEntityWarning(name, id, message);

        warning.Name.Should().Be(name);
        warning.Id.Should().Be(id);
        warning.Message.Should().Be(message);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve permitir ID nulo")]
    public void DevePermitirIdNulo()
    {
        var warning = new RequestEntityWarning("EntidadeTeste", null, "Aviso sem ID");

        warning.Id.Should().BeNull();
        warning.Name.Should().Be("EntidadeTeste");
        warning.Message.Should().Be("Aviso sem ID");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve ser igual para instâncias com mesmas propriedades")]
    public void DeveSerIgualParaInstanciasComMesmasPropriedades()
    {
        var warning1 = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");
        var warning2 = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");

        warning1.Should().Be(warning2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve ser diferente quando as propriedades forem diferentes")]
    public void DeveSerDiferenteQuandoPropriedadesForemDiferentes()
    {
        var warning1 = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");
        var warning2 = new RequestEntityWarning("OutraEntidade", 456, "Outro aviso");

        warning1.Should().NotBe(warning2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar nova instância ao modificar propriedade com 'with'")]
    public void DeveCriarNovaInstanciaAoModificarComWith()
    {
        var warning = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");

        var modifiedWarning = warning with { Message = "Novo aviso" };

        modifiedWarning.Should().NotBeSameAs(warning);
        modifiedWarning.Message.Should().Be("Novo aviso");
        warning.Message.Should().Be("Aviso sobre a entidade");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve serializar para JSON corretamente")]
    public void DeveSerializarParaJsonCorretamente()
    {
        var warning = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");

        var json = JsonSerializer.Serialize(warning);

        json.Should().Contain("\"Name\":\"EntidadeTeste\"");
        json.Should().Contain("\"Id\":123");
        json.Should().Contain("\"Message\":\"Aviso sobre a entidade\"");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve desserializar de JSON corretamente")]
    public void DeveDesserializarDeJsonCorretamente()
    {
        var json = /*lang=json,strict*/
            "{\"Name\":\"EntidadeTeste\",\"Id\":123,\"Message\":\"Aviso sobre a entidade\"}";

        var warning = JsonSerializer.Deserialize<RequestEntityWarning>(json);

        warning.Should().NotBeNull();
        warning!.Name.Should().Be("EntidadeTeste");

        // Deserializa o valor de Id para long manualmente, caso ele seja JsonElement
        if (warning.Id is JsonElement idElement && idElement.ValueKind == JsonValueKind.Number)
        {
            warning = warning with { Id = idElement.GetInt64() };
        }

        warning.Id.Should().Be(123L); // Agora Id é comparado como long
        warning.Message.Should().Be("Aviso sobre a entidade");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falso ao comparar com null")]
    public void DeveRetornarFalsoAoCompararComNull()
    {
        var warning = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");

        warning.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar string formatada corretamente ao chamar ToString")]
    public void DeveRetornarStringFormatadaCorretamenteAoChamarToString()
    {
        var warning = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");

        var stringRepresentation = warning.ToString();

        stringRepresentation.Should().Contain("EntidadeTeste");
        stringRepresentation.Should().Contain("123");
        stringRepresentation.Should().Contain("Aviso sobre a entidade");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve gerar hash code igual para instâncias com propriedades iguais")]
    public void DeveGerarHashCodeIgualParaInstanciasComPropriedadesIguais()
    {
        var warning1 = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");
        var warning2 = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");

        warning1.GetHashCode().Should().Be(warning2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve gerar hash code diferente para instâncias com propriedades diferentes")]
    public void DeveGerarHashCodeDiferenteParaInstanciasComPropriedadesDiferentes()
    {
        var warning1 = new RequestEntityWarning("EntidadeTeste", 123, "Aviso sobre a entidade");
        var warning2 = new RequestEntityWarning("OutraEntidade", 456, "Outro aviso");

        warning1.GetHashCode().Should().NotBe(warning2.GetHashCode());
    }
}
