using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;

namespace AlagaAlerta.Application.Services;

public sealed class RelatoService : IRelatoService
{
    private readonly IRelatoRepository _relatos;
    private readonly IBairroRepository _bairros;
    private readonly IPontoRiscoRepository _pontos;
    private readonly IUnitOfWork _uow;

    public RelatoService(IRelatoRepository relatos, IBairroRepository bairros, IPontoRiscoRepository pontos, IUnitOfWork uow)
    {
        _relatos = relatos;
        _bairros = bairros;
        _pontos = pontos;
        _uow = uow;
    }

    public async Task<RelatoResponse> CriarAsync(CriarRelatoRequest request, CancellationToken ct = default)
    {
        var bairroId = request.BairroId!.Value;
        _ = await _bairros.ObterPorIdAsync(bairroId, ct) ?? throw new NotFoundException("Bairro", bairroId);

        if (request.PontoRiscoId is { } pontoId)
        {
            var ponto = await _pontos.ObterPorIdAsync(pontoId, ct) ?? throw new NotFoundException("Ponto de risco", pontoId);
            if (ponto.BairroId != bairroId)
            {
                throw new DomainException("O ponto de risco informado não pertence a este bairro.");
            }
        }

        var relato = new Relato(
            bairroId,
            request.PontoRiscoId,
            request.Descricao,
            request.Intensidade!.Value,
            request.Latitude,
            request.Longitude);

        await _relatos.AdicionarAsync(relato, ct);
        await _uow.SalvarAsync(ct);
        return RelatoResponse.De(relato);
    }

    public async Task<IReadOnlyList<RelatoResponse>> ListarAsync(Guid? bairroId, StatusRelato? status, CancellationToken ct = default)
    {
        var relatos = await _relatos.ListarAsync(bairroId, status, ct);
        return relatos.Select(RelatoResponse.De).ToList();
    }

    public async Task<RelatoResponse> ModerarAsync(Guid id, StatusRelato status, CancellationToken ct = default)
    {
        var relato = await _relatos.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Relato", id);
        relato.Moderar(status);
        await _uow.SalvarAsync(ct);
        return RelatoResponse.De(relato);
    }
}
