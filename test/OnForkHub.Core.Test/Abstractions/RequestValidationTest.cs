using System.Text.Json;
using OnForkHub.Core.Abstractions;

namespace OnForkHub.Core.Test.Abstractions;

public class RequestValidationTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar RequestValidation com propriedades definidas corretamente")]
    public void DeveCriarRequestValidationComPropriedadesDefinidasCorretamente()
    {
        var propertyName = "Nome";
        var description = "Nome é obrigatório";

        var validation = new RequestValidation(propertyName, description);

        validation.PropertyName.Should().Be(propertyName);
        validation.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Dois RequestValidation com os mesmos valores devem ser iguais")]
    public void DoisRequestValidationComOsMesmosValoresDevemSerIguais()
    {
        var validation1 = new RequestValidation("Nome", "Nome é obrigatório");
        var validation2 = new RequestValidation("Nome", "Nome é obrigatório");

        validation1.Should().Be(validation2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Dois RequestValidation com valores diferentes devem ser diferentes")]
    public void DoisRequestValidationComValoresDiferentesDevemSerDiferentes()
    {
        var validation1 = new RequestValidation("Nome", "Nome é obrigatório");
        var validation2 = new RequestValidation("Idade", "Idade é obrigatória");

        validation1.Should().NotBe(validation2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar uma nova instância de RequestValidation com 'with' para alterar a descrição")]
    public void DeveCriarNovaInstanciaComWithParaAlterarDescricao()
    {
        var validation = new RequestValidation("Nome", "Nome é obrigatório");

        var updatedValidation = validation with { Description = "Nome precisa ter pelo menos 3 caracteres" };

        updatedValidation.Should().NotBeSameAs(validation);
        updatedValidation.PropertyName.Should().Be("Nome");
        updatedValidation.Description.Should().Be("Nome precisa ter pelo menos 3 caracteres");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar uma nova instância de RequestValidation com 'with' para alterar o nome da propriedade")]
    public void DeveCriarNovaInstanciaComWithParaAlterarNomePropriedade()
    {
        var validation = new RequestValidation("Nome", "Nome é obrigatório");

        var updatedValidation = validation with { PropertyName = "Idade" };

        updatedValidation.Should().NotBeSameAs(validation);
        updatedValidation.PropertyName.Should().Be("Idade");
        updatedValidation.Description.Should().Be("Nome é obrigatório");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar corretamente com nome da propriedade e descrição")]
    public void DeveInicializarCorretamenteComNomePropriedadeEDescricao()
    {
        var propertyName = "CampoTeste";
        var description = "Descrição do erro de validação";
        var validation = new RequestValidation(propertyName, description);

        validation.PropertyName.Should().Be(propertyName);
        validation.Description.Should().Be(description);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar instância equivalente quando usado operador 'with' sem modificações")]
    public void DeveRetornarInstanciaEquivalenteQuandoUsadoWithSemModificacoes()
    {
        var validation = new RequestValidation("CampoTeste", "Descrição");
        var validationCopy = validation with { };

        validationCopy.Should().Be(validation);
        validationCopy.Should().NotBeSameAs(validation);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve permitir valores nulos para propriedades")]
    public void DevePermitirValoresNulosParaPropriedades()
    {
        var validation = new RequestValidation(null, null);

        validation.PropertyName.Should().BeNull();
        validation.Description.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve ser diferente quando uma propriedade for nula")]
    public void DeveSerDiferenteQuandoUmaPropriedadeForNula()
    {
        var validation1 = new RequestValidation("CampoTeste", null);
        var validation2 = new RequestValidation("CampoTeste", "Descrição");

        validation1.Should().NotBe(validation2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve serializar para JSON corretamente")]
    public void DeveSerializarParaJsonCorretamente()
    {
        var validation = new RequestValidation("CampoTeste", "Descrição do erro");
        var json = JsonSerializer.Serialize(validation);

        var deserializedValidation = JsonSerializer.Deserialize<RequestValidation>(json);

        deserializedValidation.Should().NotBeNull();
        deserializedValidation!.PropertyName.Should().Be("CampoTeste");
        deserializedValidation.Description.Should().Be("Descrição do erro");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve desserializar de JSON corretamente")]
    public void DeveDesserializarDeJsonCorretamente()
    {
        var json = /*lang=json,strict*/
            "{\"PropertyName\":\"CampoTeste\",\"Description\":\"Descrição do erro\"}";
        var validation = JsonSerializer.Deserialize<RequestValidation>(json);

        validation.Should().NotBeNull();
        validation!.PropertyName.Should().Be("CampoTeste");
        validation.Description.Should().Be("Descrição do erro");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve suportar comparação explícita usando Equals")]
    public void DeveSuportarComparacaoExplicitaUsandoEquals()
    {
        var validation1 = new RequestValidation("CampoTeste", "Descrição");
        var validation2 = new RequestValidation("CampoTeste", "Descrição");

        validation1.Equals(validation2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falso ao comparar instância com nulo")]
    public void DeveRetornarFalsoAoCompararInstanciaComNulo()
    {
        var validation = new RequestValidation("CampoTeste", "Descrição");

        validation.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar string formatada corretamente ao chamar ToString")]
    public void DeveRetornarStringFormatadaCorretamenteAoChamarToString()
    {
        var validation = new RequestValidation("CampoTeste", "Descrição do erro");
        var stringRepresentation = validation.ToString();

        stringRepresentation.Should().Contain("CampoTeste");
        stringRepresentation.Should().Contain("Descrição do erro");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve ser considerado diferente de outro tipo ao usar Equals")]
    public void DeveSerConsideradoDiferenteDeOutroTipoAoUsarEquals()
    {
        var validation = new RequestValidation("CampoTeste", "Descrição");

        validation.Equals("OutroTipo").Should().BeFalse();
    }
}
