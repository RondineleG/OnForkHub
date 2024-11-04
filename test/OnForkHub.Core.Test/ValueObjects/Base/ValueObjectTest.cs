using OnForkHub.Core.ValueObjects.Base;

namespace OnForkHub.Core.Test.ValueObjects.Base;

public class SampleValueObject(int property1, string property2) : ValueObject
{
    public int Property1 { get; } = property1;
    public string Property2 { get; } = property2;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Property1;
        yield return Property2;
    }
}

public class ValueObjectTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve considerar objetos como iguais quando propriedades são iguais")]
    public void DeveConsiderarObjetosComoIguaisQuandoPropriedadesSaoIguais()
    {
        var obj1 = new SampleValueObject(1, "Test");
        var obj2 = new SampleValueObject(1, "Test");

        obj1.Equals(obj2).Should().BeTrue();
        obj1.Should().Be(obj2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve considerar objetos como diferentes quando propriedades são diferentes")]
    public void DeveConsiderarObjetosComoDiferentesQuandoPropriedadesSaoDiferentes()
    {
        var obj1 = new SampleValueObject(1, "Test");
        var obj2 = new SampleValueObject(2, "Different");

        obj1.Equals(obj2).Should().BeFalse();
        obj1.Should().NotBe(obj2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falso ao comparar com null")]
    public void DeveRetornarFalsoAoCompararComNull()
    {
        var obj1 = new SampleValueObject(1, "Test");

        obj1.Equals(null).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar hash code igual para objetos com propriedades iguais")]
    public void DeveRetornarHashCodeIgualParaObjetosComPropriedadesIguais()
    {
        var obj1 = new SampleValueObject(1, "Test");
        var obj2 = new SampleValueObject(1, "Test");

        obj1.GetHashCode().Should().Be(obj2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar hash code diferente para objetos com propriedades diferentes")]
    public void DeveRetornarHashCodeDiferenteParaObjetosComPropriedadesDiferentes()
    {
        var obj1 = new SampleValueObject(1, "Test");
        var obj2 = new SampleValueObject(2, "Different");

        obj1.GetHashCode().Should().NotBe(obj2.GetHashCode());
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falso ao comparar tipos diferentes")]
    public void DeveRetornarFalsoAoCompararTiposDiferentes()
    {
        var obj1 = new SampleValueObject(1, "Test");
        var differentTypeObj = "I am not a ValueObject";

        obj1.Equals(differentTypeObj).Should().BeFalse();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve considerar objetos como iguais quando ambos são null")]
    public void DeveConsiderarObjetosComoIguaisQuandoAmbosSaoNull()
    {
        SampleValueObject obj1 = null;
        SampleValueObject obj2 = null;

        ValueObject.EqualOperator(obj1, obj2).Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve retornar falso ao comparar um objeto null com um não-null")]
    public void DeveRetornarFalsoAoCompararUmObjetoNullComUmNaoNull()
    {
        var obj1 = new SampleValueObject(1, "Test");
        SampleValueObject obj2 = null;

        ValueObject.EqualOperator(obj1, obj2).Should().BeFalse();
        ValueObject.EqualOperator(obj2, obj1).Should().BeFalse();
    }
}
