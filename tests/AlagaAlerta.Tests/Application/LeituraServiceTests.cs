using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Application.Services;
using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;
using AlagaAlerta.Domain.ValueObjects;
using NSubstitute;

namespace AlagaAlerta.Tests.Application;

public class LeituraServiceTests
{
    private readonly ISensorRepository _sensores = Substitute.For<ISensorRepository>();
    private readonly IPontoRiscoRepository _pontos = Substitute.For<IPontoRiscoRepository>();
    private readonly ILeituraRepository _leituras = Substitute.For<ILeituraRepository>();
    private readonly IAlertaRepository _alertas = Substitute.For<IAlertaRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly RelogioFixo _relogio = new(new DateTimeOffset(2026, 11, 10, 18, 0, 0, TimeSpan.Zero));
    private readonly PontoRisco _ponto;
    private readonly Sensor _sensor;
    private readonly LeituraService _service;

    public LeituraServiceTests()
    {
        _ponto = new PontoRisco("Av. Pompeia", null, -23.53, -46.68, new LimitesAlagamento(15, 30), Guid.NewGuid());
        _sensor = new Sensor("ESP32-01", _ponto.Id);

        _sensores.ObterPorCodigoAsync("ESP32-01", Arg.Any<CancellationToken>()).Returns(_sensor);
        _pontos.ObterPorIdAsync(_ponto.Id, Arg.Any<CancellationToken>()).Returns(_ponto);

        _service = new LeituraService(_sensores, _pontos, _leituras, _alertas, _uow, _relogio);
    }

    private Task<ResultadoLeituraResponse> Enviar(double nivel, string codigo = "esp32-01") =>
        _service.RegistrarAsync(new RegistrarLeituraRequest { CodigoSensor = codigo, NivelAguaCm = nivel });

    private void AlertaAtivo(Alerta? alerta) =>
        _alertas.ObterAtivoDoPontoAsync(_ponto.Id, Arg.Any<CancellationToken>()).Returns(alerta);

    [Fact]
    public async Task Nivel_normal_sem_alerta_nao_faz_nada()
    {
        AlertaAtivo(null);

        var resultado = await Enviar(5);

        Assert.Equal(NivelRisco.Normal, resultado.Nivel);
        Assert.Equal(AcaoAlerta.Nenhuma, resultado.Acao);
        Assert.Null(resultado.Alerta);
        await _alertas.DidNotReceive().AdicionarAsync(Arg.Any<Alerta>(), Arg.Any<CancellationToken>());
        await _leituras.Received(1).AdicionarAsync(Arg.Any<Leitura>(), Arg.Any<CancellationToken>());
        await _uow.Received(1).SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Passar_da_atencao_cria_alerta()
    {
        AlertaAtivo(null);

        var resultado = await Enviar(18);

        Assert.Equal(AcaoAlerta.Criado, resultado.Acao);
        Assert.Equal(NivelAlerta.Atencao, resultado.Alerta!.Nivel);
        await _alertas.Received(1).AdicionarAsync(Arg.Is<Alerta>(a => a.Nivel == NivelAlerta.Atencao), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Mesmo_nivel_mantem_o_alerta_sem_duplicar()
    {
        var existente = Alerta.Emitir(_ponto, NivelAlerta.Atencao, 18, _relogio.Agora.UtcDateTime);
        AlertaAtivo(existente);

        var resultado = await Enviar(22);

        Assert.Equal(AcaoAlerta.Mantido, resultado.Acao);
        Assert.Equal(existente.Id, resultado.Alerta!.Id);
        await _alertas.DidNotReceive().AdicionarAsync(Arg.Any<Alerta>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Escalar_para_critico_encerra_o_alerta_anterior()
    {
        var existente = Alerta.Emitir(_ponto, NivelAlerta.Atencao, 18, _relogio.Agora.UtcDateTime);
        AlertaAtivo(existente);
        _relogio.Avancar(TimeSpan.FromMinutes(10));

        var resultado = await Enviar(35);

        Assert.Equal(AcaoAlerta.Atualizado, resultado.Acao);
        Assert.Equal(NivelAlerta.Critico, resultado.Alerta!.Nivel);
        Assert.False(existente.Ativo);
        Assert.Equal(_relogio.Agora.UtcDateTime, existente.EncerradoEm);
    }

    [Fact]
    public async Task Agua_baixando_encerra_o_alerta()
    {
        var existente = Alerta.Emitir(_ponto, NivelAlerta.Critico, 35, _relogio.Agora.UtcDateTime);
        AlertaAtivo(existente);

        var resultado = await Enviar(3);

        Assert.Equal(AcaoAlerta.Encerrado, resultado.Acao);
        Assert.False(resultado.Alerta!.Ativo);
        Assert.NotNull(resultado.Alerta.EncerradoEm);
    }

    [Fact]
    public async Task Sensor_desconhecido_gera_404()
    {
        await Assert.ThrowsAsync<NotFoundException>(() => Enviar(10, "NAO-EXISTE"));
        await _uow.DidNotReceive().SalvarAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Sensor_inativo_gera_erro_de_regra()
    {
        _sensor.Desativar();

        var erro = await Assert.ThrowsAsync<DomainException>(() => Enviar(10));

        Assert.Equal(422, erro.StatusCode);
    }
}
