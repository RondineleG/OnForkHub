namespace OnForkHub.Core.Test.Entities.Base;

public class BaseEntityTests
{
    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve definir CreatedAt corretamente ao instanciar a entidade")]
    public void CreatedAtNaoDeveSerDefaultAoInstanciarEntidade()
    {
        var entidade = new EntidadeValidaTestFixture();
        entidade.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve atualizar UpdatedAt quando executar método Update")]
    public void DeveAtualizarDataAtualizacaoQuandoExecutarUpdate()
    {
        var entidade = new EntidadeValidaTestFixture();
        var antesDoUpdate = DateTime.UtcNow;

        entidade.ExecutarUpdate();

        entidade.UpdatedAt.Should().NotBeNull();
        entidade.UpdatedAt.Should().BeCloseTo(antesDoUpdate, TimeSpan.FromSeconds(1));
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve atualizar UpdatedAt para horário recente ao chamar Update várias vezes")]
    public void DeveAtualizarUpdatedAtParaHorarioRecenteAposExecutarUpdateVariasVezes()
    {
        var entidade = new EntidadeValidaTestFixture();

        entidade.ExecutarUpdate();
        var primeiraAtualizacao = entidade.UpdatedAt;

        Thread.Sleep(100);

        entidade.ExecutarUpdate();
        var segundaAtualizacao = entidade.UpdatedAt;

        primeiraAtualizacao.Should().NotBe(segundaAtualizacao);
        segundaAtualizacao.Should().BeAfter(primeiraAtualizacao.Value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(long.MaxValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Deve criar entidade com ID válido")]
    public void DeveCriarEntidadeQuandoIdForValido(long id)
    {
        var dataCriacao = DateTime.UtcNow;

        var entidade = new EntidadeValidaTestFixture(id, dataCriacao);

        entidade.Id.Should().Be(id);
        entidade.CreatedAt.Should().Be(dataCriacao);
        entidade.UpdatedAt.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve definir todas as propriedades ao fornecer uma data de atualização")]
    public void DeveDefinirTodasPropriedadesQuandoInformarDataAtualizacao()
    {
        var id = 1L;
        var dataCriacao = DateTime.UtcNow.AddDays(-1);
        var dataAtualizacao = DateTime.UtcNow;

        var entidade = new EntidadeValidaTestFixture(id, dataCriacao, dataAtualizacao);

        entidade.Id.Should().Be(id);
        entidade.CreatedAt.Should().Be(dataCriacao);
        entidade.UpdatedAt.Should().Be(dataAtualizacao);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve inicializar propriedades ao usar o construtor padrão")]
    public void DeveInicializarPropriedadesQuandoUsarConstrutorPadrao()
    {
        var entidade = new EntidadeValidaTestFixture();
        var dataAtual = DateTime.UtcNow;

        entidade.CreatedAt.Should().BeCloseTo(dataAtual, TimeSpan.FromSeconds(1));
        entidade.UpdatedAt.Should().BeNull();
        entidade.Id.Should().Be(0);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção com mensagem específica para ID inválido")]
    public void DeveLancarExcecaoComMensagemEspecificaQuandoIdForInvalido(long id)
    {
        var dataCriacao = DateTime.UtcNow;

        Action acao = () => new EntidadeValidaTestFixture(id, dataCriacao);

        acao.Should().Throw<DomainException>().WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção para ID com valor negativo máximo")]
    public void DeveLancarExcecaoParaIdComValorMaximoNegativo()
    {
        var dataCriacao = DateTime.UtcNow;

        Action acao = () => new EntidadeValidaTestFixture(-long.MaxValue, dataCriacao);

        acao.Should().Throw<DomainException>().WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção quando entidade não for válida ao executar método Update")]
    public void DeveLancarExcecaoQuandoEntidadeNaoForValidaAoExecutarUpdate()
    {
        var entidade = new EntidadeInvalidaTestFixture();

        Action acao = entidade.ExecutarUpdate;

        acao.Should().Throw<DomainException>().WithMessage("Id deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    [Trait("Category", "Unit")]
    [DisplayName("Deve lançar exceção para ID inválido")]
    public void DeveLancarExcecaoQuandoIdForInvalido(long id)
    {
        var dataCriacao = DateTime.UtcNow;

        Action acao = () => new EntidadeValidaTestFixture(id, dataCriacao);

        acao.Should().Throw<DomainException>().WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Deve manter CreatedAt no fuso horário UTC")]
    public void DeveManterCreatedAtNoFusoHorarioUtc()
    {
        var dataCriacao = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var entidade = new EntidadeValidaTestFixture(1, dataCriacao);

        entidade.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entidade.CreatedAt.Should().Be(dataCriacao);
    }

    [Fact]
    [Trait("Category", "Unit")]
    [DisplayName("Não deve alterar UpdatedAt sem chamar método Update")]
    public void NaoDeveAlterarUpdatedAtSemChamarMetodoUpdate()
    {
        var entidade = new EntidadeValidaTestFixture();
        entidade.UpdatedAt.Should().BeNull();
    }
}
