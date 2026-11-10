using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;

namespace AlagaAlerta.Application.Services;

public sealed class LeituraService : ILeituraService
{
    private readonly ISensorRepository _sensores;
    private readonly IPontoRiscoRepository _pontos;
    private readonly ILeituraRepository _leituras;
    private readonly IAlertaRepository _alertas;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _relogio;

    public LeituraService(
        ISensorRepository sensores,
        IPontoRiscoRepository pontos,
        ILeituraRepository leituras,
        IAlertaRepository alertas,
        IUnitOfWork uow,
        TimeProvider relogio)
    {
        _sensores = sensores;
        _pontos = pontos;
        _leituras = leituras;
        _alertas = alertas;
        _uow = uow;
        _relogio = relogio;
    }

    public async Task<ResultadoLeituraResponse> RegistrarAsync(RegistrarLeituraRequest request, CancellationToken ct = default)
    {
        var codigo = Sensor.NormalizarCodigo(request.CodigoSensor);
        var sensor = await _sensores.ObterPorCodigoAsync(codigo, ct) ?? throw new NotFoundException("Sensor", codigo);
        var ponto = await _pontos.ObterPorIdAsync(sensor.PontoRiscoId, ct)
            ?? throw new NotFoundException("Ponto de risco", sensor.PontoRiscoId);

        var agora = _relogio.GetUtcNow().UtcDateTime;
        var leitura = sensor.RegistrarLeitura(
            request.NivelAguaCm!.Value,
            request.ChuvaMmHora,
            request.RegistradaEm?.UtcDateTime ?? agora);
        await _leituras.AdicionarAsync(leitura, ct);

        var nivel = ponto.Classificar(leitura.NivelAguaCm);
        var alertaAtivo = await _alertas.ObterAtivoDoPontoAsync(ponto.Id, ct);
        var (acao, alerta) = await AvaliarAlertaAsync(ponto, alertaAtivo, nivel, leitura.NivelAguaCm, agora, ct);

        await _uow.SalvarAsync(ct);

        return new ResultadoLeituraResponse(
            LeituraResponse.De(leitura),
            nivel,
            acao,
            alerta is null ? null : AlertaResponse.De(alerta));
    }

    private async Task<(AcaoAlerta Acao, Alerta? Alerta)> AvaliarAlertaAsync(
        PontoRisco ponto,
        Alerta? alertaAtivo,
        NivelRisco nivel,
        double nivelAguaCm,
        DateTime agora,
        CancellationToken ct)
    {
        if (!nivel.GeraAlerta())
        {
            if (alertaAtivo is null)
            {
                return (AcaoAlerta.Nenhuma, null);
            }
            alertaAtivo.Encerrar(agora);
            return (AcaoAlerta.Encerrado, alertaAtivo);
        }

        var nivelAlerta = nivel.ParaNivelAlerta();
        if (alertaAtivo is not null && alertaAtivo.Nivel == nivelAlerta)
        {
            return (AcaoAlerta.Mantido, alertaAtivo);
        }

        alertaAtivo?.Encerrar(agora);
        var novo = Alerta.Emitir(ponto, nivelAlerta, nivelAguaCm, agora);
        await _alertas.AdicionarAsync(novo, ct);
        return (alertaAtivo is null ? AcaoAlerta.Criado : AcaoAlerta.Atualizado, novo);
    }
}
