namespace OnForkHub.Core.Test.Entities.Base;

public class BaseEntityTests
{
    [Fact]
    public void CreatedAtNaoDeveSerDefaultAoInstanciarEntidade()
    {
        var entidade = new EntidadeValidaTestFixture();
        entidade.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void DeveAtualizarDataAtualizacaoQuandoExecutarUpdate()
    {
        var entidade = new EntidadeValidaTestFixture();
        var antesDoUpdate = DateTime.UtcNow;

        entidade.ExecutarUpdate();

        entidade.UpdatedAt.Should().NotBeNull();
        entidade.UpdatedAt.Should().BeCloseTo(antesDoUpdate, TimeSpan.FromSeconds(1));
    }

    [Fact]
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
    public void DeveCriarEntidadeQuandoIdForValido(long id)
    {
        var dataCriacao = DateTime.UtcNow;

        var entidade = new EntidadeValidaTestFixture(id, dataCriacao);

        entidade.Id.Should().Be(id);
        entidade.CreatedAt.Should().Be(dataCriacao);
        entidade.UpdatedAt.Should().BeNull();
    }

    [Fact]
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
    public void DeveLancarExcecaoComMensagemEspecificaQuandoIdForInvalido(long id)
    {
        var dataCriacao = DateTime.UtcNow;

        Action acao = () => new EntidadeValidaTestFixture(id, dataCriacao);

        acao.Should()
           .Throw<DomainException>()
           .WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    public void DeveLancarExcecaoParaIdComValorMaximoNegativo()
    {
        var dataCriacao = DateTime.UtcNow;

        Action acao = () => new EntidadeValidaTestFixture(-long.MaxValue, dataCriacao);

        acao.Should()
           .Throw<DomainException>()
           .WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    public void DeveLancarExcecaoQuandoEntidadeNaoForValidaAoExecutarUpdate()
    {
        var entidade = new EntidadeInvalidaTestFixture();

        Action acao = () => entidade.ExecutarUpdate();

        acao.Should().Throw<DomainException>()
            .WithMessage("Id deve ser maior que zero");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(long.MinValue)]
    public void DeveLancarExcecaoQuandoIdForInvalido(long id)
    {
        var dataCriacao = DateTime.UtcNow;

        Action acao = () => new EntidadeValidaTestFixture(id, dataCriacao);

        acao.Should()
           .Throw<DomainException>()
           .WithMessage("Id deve ser maior que zero");
    }

    [Fact]
    public void DeveManterCreatedAtNoFusoHorarioUtc()
    {
        var dataCriacao = new DateTime(2022, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        var entidade = new EntidadeValidaTestFixture(1, dataCriacao);

        entidade.CreatedAt.Kind.Should().Be(DateTimeKind.Utc);
        entidade.CreatedAt.Should().Be(dataCriacao);
    }

    [Fact]
    public void NaoDeveAlterarUpdatedAtSemChamarMetodoUpdate()
    {
        var entidade = new EntidadeValidaTestFixture();
        entidade.UpdatedAt.Should().BeNull();
    }
}