using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Repositories;
using AlagaAlerta.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlagaAlerta.Infrastructure.Repositories;

public abstract class RepositorioBase<T> : IRepositorio<T> where T : Entidade
{
    protected RepositorioBase(AlagaAlertaDbContext db)
    {
        Db = db;
    }

    protected AlagaAlertaDbContext Db { get; }
    protected DbSet<T> Set => Db.Set<T>();

    public virtual async Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default) =>
        await Set.FirstOrDefaultAsync(e => e.Id == id, ct);

    public async Task AdicionarAsync(T entidade, CancellationToken ct = default) =>
        await Set.AddAsync(entidade, ct);

    public void Remover(T entidade) => Set.Remove(entidade);
}

public sealed class BairroRepository : RepositorioBase<Bairro>, IBairroRepository
{
    public BairroRepository(AlagaAlertaDbContext db) : base(db)
    {
    }

    public Task<bool> ExisteComNomeAsync(string nome, CancellationToken ct = default)
    {
        var normalizado = nome.Trim().ToLower();
        return Set.AnyAsync(b => b.Nome.ToLower() == normalizado, ct);
    }

    public async Task<IReadOnlyList<Bairro>> ListarAsync(CancellationToken ct = default) =>
        await Set.AsNoTracking().OrderBy(b => b.Nome).ToListAsync(ct);
}

public sealed class PontoRiscoRepository : RepositorioBase<PontoRisco>, IPontoRiscoRepository
{
    public PontoRiscoRepository(AlagaAlertaDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<PontoRisco>> ListarAsync(Guid? bairroId, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(p => bairroId == null || p.BairroId == bairroId)
            .OrderBy(p => p.Nome)
            .ToListAsync(ct);
}

public sealed class SensorRepository : RepositorioBase<Sensor>, ISensorRepository
{
    public SensorRepository(AlagaAlertaDbContext db) : base(db)
    {
    }

    public Task<Sensor?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default) =>
        Set.FirstOrDefaultAsync(s => s.Codigo == codigo, ct);

    public async Task<IReadOnlyList<Sensor>> ListarAsync(Guid? pontoRiscoId, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(s => pontoRiscoId == null || s.PontoRiscoId == pontoRiscoId)
            .OrderBy(s => s.Codigo)
            .ToListAsync(ct);
}

public sealed class LeituraRepository : RepositorioBase<Leitura>, ILeituraRepository
{
    public LeituraRepository(AlagaAlertaDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<Leitura>> ListarPorSensorAsync(Guid sensorId, int limite, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(l => l.SensorId == sensorId)
            .OrderByDescending(l => l.RegistradaEm)
            .Take(limite)
            .ToListAsync(ct);

    public Task<Leitura?> ObterUltimaDoPontoAsync(Guid pontoRiscoId, CancellationToken ct = default) =>
        Set.AsNoTracking()
            .Where(l => l.Sensor!.PontoRiscoId == pontoRiscoId)
            .OrderByDescending(l => l.RegistradaEm)
            .FirstOrDefaultAsync(ct);
}

public sealed class AlertaRepository : RepositorioBase<Alerta>, IAlertaRepository
{
    public AlertaRepository(AlagaAlertaDbContext db) : base(db)
    {
    }

    public Task<Alerta?> ObterAtivoDoPontoAsync(Guid pontoRiscoId, CancellationToken ct = default) =>
        Set.Where(a => a.PontoRiscoId == pontoRiscoId && a.Ativo)
            .OrderByDescending(a => a.CriadoEm)
            .FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<Alerta>> ListarAsync(bool? ativos, Guid? bairroId, CancellationToken ct = default)
    {
        var consulta = Set.AsNoTracking();

        if (ativos.HasValue)
        {
            consulta = consulta.Where(a => a.Ativo == ativos.Value);
        }
        if (bairroId.HasValue)
        {
            consulta = consulta.Where(a => a.PontoRisco!.BairroId == bairroId.Value);
        }

        return await consulta.OrderByDescending(a => a.CriadoEm).ToListAsync(ct);
    }
}

public sealed class RelatoRepository : RepositorioBase<Relato>, IRelatoRepository
{
    public RelatoRepository(AlagaAlertaDbContext db) : base(db)
    {
    }

    public async Task<IReadOnlyList<Relato>> ListarAsync(Guid? bairroId, StatusRelato? status, CancellationToken ct = default) =>
        await Set.AsNoTracking()
            .Where(r => bairroId == null || r.BairroId == bairroId)
            .Where(r => status == null || r.Status == status)
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(ct);
}
