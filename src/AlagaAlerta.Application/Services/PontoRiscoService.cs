using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;
using AlagaAlerta.Domain.ValueObjects;

namespace AlagaAlerta.Application.Services;

public sealed class PontoRiscoService : IPontoRiscoService
{
    private readonly IPontoRiscoRepository _pontos;
    private readonly IBairroRepository _bairros;
    private readonly ILeituraRepository _leituras;
    private readonly IAlertaRepository _alertas;
    private readonly IUnitOfWork _uow;

    public PontoRiscoService(
        IPontoRiscoRepository pontos,
        IBairroRepository bairros,
        ILeituraRepository leituras,
        IAlertaRepository alertas,
        IUnitOfWork uow)
    {
        _pontos = pontos;
        _bairros = bairros;
        _leituras = leituras;
        _alertas = alertas;
        _uow = uow;
    }

    public async Task<PontoRiscoResponse> CriarAsync(CriarPontoRiscoRequest request, CancellationToken ct = default)
    {
        var limites = new LimitesAlagamento(request.NivelAtencaoCm!.Value, request.NivelCriticoCm!.Value);
        await GarantirBairroAsync(request.BairroId!.Value, ct);

        var ponto = new PontoRisco(
            request.Nome,
            request.Descricao,
            request.Latitude!.Value,
            request.Longitude!.Value,
            limites,
            request.BairroId.Value);

        await _pontos.AdicionarAsync(ponto, ct);
        await _uow.SalvarAsync(ct);
        return PontoRiscoResponse.De(ponto);
    }

    public async Task<IReadOnlyList<PontoRiscoResponse>> ListarAsync(Guid? bairroId, CancellationToken ct = default)
    {
        var pontos = await _pontos.ListarAsync(bairroId, ct);
        return pontos.Select(PontoRiscoResponse.De).ToList();
    }

    public async Task<PontoRiscoResponse> BuscarAsync(Guid id, CancellationToken ct = default) =>
        PontoRiscoResponse.De(await ObterAsync(id, ct));

    public async Task<PontoRiscoResponse> AtualizarAsync(Guid id, AtualizarPontoRiscoRequest request, CancellationToken ct = default)
    {
        var ponto = await ObterAsync(id, ct);
        if (request.BairroId.HasValue)
        {
            await GarantirBairroAsync(request.BairroId.Value, ct);
        }

        ponto.Atualizar(
            request.Nome,
            request.Descricao,
            request.Latitude,
            request.Longitude,
            request.NivelAtencaoCm,
            request.NivelCriticoCm,
            request.BairroId);

        await _uow.SalvarAsync(ct);
        return PontoRiscoResponse.De(ponto);
    }

    public async Task RemoverAsync(Guid id, CancellationToken ct = default)
    {
        var ponto = await ObterAsync(id, ct);
        _pontos.Remover(ponto);
        await _uow.SalvarAsync(ct);
    }

    public async Task<StatusPontoResponse> StatusAsync(Guid id, CancellationToken ct = default)
    {
        var ponto = await ObterAsync(id, ct);
        var ultimaLeitura = await _leituras.ObterUltimaDoPontoAsync(id, ct);
        var alertaAtivo = await _alertas.ObterAtivoDoPontoAsync(id, ct);

        var nivelAtual = ultimaLeitura is null ? NivelRisco.SemDados : ponto.Classificar(ultimaLeitura.NivelAguaCm);

        return new StatusPontoResponse(
            PontoRiscoResponse.De(ponto),
            nivelAtual,
            ultimaLeitura is null ? null : LeituraResponse.De(ultimaLeitura),
            alertaAtivo is null ? null : AlertaResponse.De(alertaAtivo));
    }

    private async Task<PontoRisco> ObterAsync(Guid id, CancellationToken ct) =>
        await _pontos.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Ponto de risco", id);

    private async Task GarantirBairroAsync(Guid bairroId, CancellationToken ct)
    {
        _ = await _bairros.ObterPorIdAsync(bairroId, ct) ?? throw new NotFoundException("Bairro", bairroId);
    }
}
