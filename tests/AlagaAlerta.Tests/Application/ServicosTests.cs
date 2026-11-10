using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Application.Services;
using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;
using AlagaAlerta.Domain.ValueObjects;
using NSubstitute;

namespace AlagaAlerta.Tests.Application;

public class ServicosTests
{
    private readonly IBairroRepository _bairros = Substitute.For<IBairroRepository>();
    private readonly IPontoRiscoRepository _pontos = Substitute.For<IPontoRiscoRepository>();
    private readonly IRelatoRepository _relatos = Substitute.For<IRelatoRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    [Fact]
    public async Task Nao_permite_bairro_com_nome_repetido()
    {
        _bairros.ExisteComNomeAsync("Sé", Arg.Any<CancellationToken>()).Returns(true);
        var service = new BairroService(_bairros, _uow);

        await Assert.ThrowsAsync<ConflictException>(() =>
            service.CriarAsync(new CriarBairroRequest { Nome = " Sé ", Zona = Zona.Centro }));
        await _uow.DidNotReceive().SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Cria_bairro_e_salva()
    {
        var service = new BairroService(_bairros, _uow);

        var bairro = await service.CriarAsync(new CriarBairroRequest { Nome = "Lapa", Zona = Zona.Oeste });

        Assert.Equal("Lapa", bairro.Nome);
        await _bairros.Received(1).AdicionarAsync(Arg.Any<Bairro>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Nao_cria_ponto_em_bairro_inexistente()
    {
        var service = new PontoRiscoService(_pontos, _bairros, Substitute.For<ILeituraRepository>(), Substitute.For<IAlertaRepository>(), _uow);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CriarAsync(new CriarPontoRiscoRequest
        {
            Nome = "Ponto X",
            Latitude = 0,
            Longitude = 0,
            NivelAtencaoCm = 10,
            NivelCriticoCm = 20,
            BairroId = Guid.NewGuid()
        }));
    }

    [Fact]
    public async Task Relato_nao_pode_apontar_ponto_de_outro_bairro()
    {
        var bairro1 = new Bairro("Bairro 1", Zona.Sul);
        var bairro2 = new Bairro("Bairro 2", Zona.Sul);
        var ponto = new PontoRisco("Ponto do bairro 1", null, 0, 0, new LimitesAlagamento(10, 20), bairro1.Id);
        _bairros.ObterPorIdAsync(bairro2.Id, Arg.Any<CancellationToken>()).Returns(bairro2);
        _pontos.ObterPorIdAsync(ponto.Id, Arg.Any<CancellationToken>()).Returns(ponto);
        var service = new RelatoService(_relatos, _bairros, _pontos, _uow);

        await Assert.ThrowsAsync<DomainException>(() => service.CriarAsync(new CriarRelatoRequest
        {
            BairroId = bairro2.Id,
            PontoRiscoId = ponto.Id,
            Descricao = "Alagou aqui",
            Intensidade = IntensidadeRelato.Baixa
        }));
        await _relatos.DidNotReceive().AdicionarAsync(Arg.Any<Relato>(), Arg.Any<CancellationToken>());
    }
}
