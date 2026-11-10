using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;

namespace AlagaAlerta.Application.Services;

public sealed class SensorService : ISensorService
{
    private readonly ISensorRepository _sensores;
    private readonly IPontoRiscoRepository _pontos;
    private readonly ILeituraRepository _leituras;
    private readonly IUnitOfWork _uow;

    public SensorService(ISensorRepository sensores, IPontoRiscoRepository pontos, ILeituraRepository leituras, IUnitOfWork uow)
    {
        _sensores = sensores;
        _pontos = pontos;
        _leituras = leituras;
        _uow = uow;
    }

    public async Task<SensorResponse> CriarAsync(CriarSensorRequest request, CancellationToken ct = default)
    {
        var pontoId = request.PontoRiscoId!.Value;
        _ = await _pontos.ObterPorIdAsync(pontoId, ct) ?? throw new NotFoundException("Ponto de risco", pontoId);

        var codigo = Sensor.NormalizarCodigo(request.Codigo);
        if (await _sensores.ObterPorCodigoAsync(codigo, ct) is not null)
        {
            throw new ConflictException($"Já existe um sensor com o código {codigo}.");
        }

        var sensor = new Sensor(codigo, pontoId, request.Ativo);
        await _sensores.AdicionarAsync(sensor, ct);
        await _uow.SalvarAsync(ct);
        return SensorResponse.De(sensor);
    }

    public async Task<IReadOnlyList<SensorResponse>> ListarAsync(Guid? pontoRiscoId, CancellationToken ct = default)
    {
        var sensores = await _sensores.ListarAsync(pontoRiscoId, ct);
        return sensores.Select(SensorResponse.De).ToList();
    }

    public async Task<SensorResponse> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken ct = default)
    {
        var sensor = await ObterAsync(id, ct);
        if (ativo)
        {
            sensor.Ativar();
        }
        else
        {
            sensor.Desativar();
        }
        await _uow.SalvarAsync(ct);
        return SensorResponse.De(sensor);
    }

    public async Task<IReadOnlyList<LeituraResponse>> HistoricoAsync(Guid id, int limite, CancellationToken ct = default)
    {
        await ObterAsync(id, ct);
        var leituras = await _leituras.ListarPorSensorAsync(id, Math.Clamp(limite, 1, 500), ct);
        return leituras.Select(LeituraResponse.De).ToList();
    }

    private async Task<Sensor> ObterAsync(Guid id, CancellationToken ct) =>
        await _sensores.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Sensor", id);
}
