using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain;
using AlagaAlerta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlagaAlerta.Infrastructure.Repositories;

public sealed class PainelConsultas : IPainelConsultas
{
    private readonly AlagaAlertaDbContext _db;

    public PainelConsultas(AlagaAlertaDbContext db) => _db = db;

    public async Task<IReadOnlyList<IndicadoresBairro>> IndicadoresPorBairroAsync(CancellationToken ct = default) =>
        await _db.Bairros.AsNoTracking()
            .OrderBy(b => b.Nome)
            .Select(b => new IndicadoresBairro(
                b.Id,
                b.Nome,
                b.Zona,
                _db.PontosRisco.Count(p => p.BairroId == b.Id),
                _db.Sensores.Count(s => s.Ativo && s.PontoRisco!.BairroId == b.Id),
                _db.Alertas.Count(a => a.Ativo && a.PontoRisco!.BairroId == b.Id),
                _db.Alertas.Count(a => a.PontoRisco!.BairroId == b.Id),
                _db.Relatos.Count(r => r.BairroId == b.Id && r.Status == StatusRelato.Confirmado),
                _db.Relatos.Count(r => r.BairroId == b.Id && r.Status == StatusRelato.Aberto)))
            .ToListAsync(ct);

    public async Task<IReadOnlyList<PontoMaisAlertado>> PontosMaisAlertadosAsync(int quantidade, CancellationToken ct = default)
    {
        var linhas = await _db.PontosRisco.AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Nome,
                Bairro = p.Bairro!.Nome,
                Total = _db.Alertas.Count(a => a.PontoRiscoId == p.Id),
                Criticos = _db.Alertas.Count(a => a.PontoRiscoId == p.Id && a.Nivel == NivelAlerta.Critico),
                Ultimo = _db.Alertas.Where(a => a.PontoRiscoId == p.Id).Max(a => (DateTime?)a.CriadoEm)
            })
            .Where(x => x.Total > 0)
            .OrderByDescending(x => x.Total)
            .ThenBy(x => x.Nome)
            .Take(quantidade)
            .ToListAsync(ct);

        return linhas
            .Select(x => new PontoMaisAlertado(x.Id, x.Nome, x.Bairro, x.Total, x.Criticos, x.Ultimo))
            .ToList();
    }

    public async Task<IReadOnlyList<TimeSpan>> DuracoesAlertasEncerradosAsync(DateTime desde, CancellationToken ct = default)
    {
        var periodos = await _db.Alertas.AsNoTracking()
            .Where(a => !a.Ativo && a.EncerradoEm != null && a.CriadoEm >= desde)
            .Select(a => new { a.CriadoEm, EncerradoEm = a.EncerradoEm!.Value })
            .ToListAsync(ct);

        return periodos.Select(p => p.EncerradoEm - p.CriadoEm).ToList();
    }
}
