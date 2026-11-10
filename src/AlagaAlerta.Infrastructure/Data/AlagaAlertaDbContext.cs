using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace AlagaAlerta.Infrastructure.Data;

public class AlagaAlertaDbContext : DbContext, IUnitOfWork
{
    public AlagaAlertaDbContext(DbContextOptions<AlagaAlertaDbContext> options) : base(options)
    {
    }

    public DbSet<Bairro> Bairros => Set<Bairro>();
    public DbSet<PontoRisco> PontosRisco => Set<PontoRisco>();
    public DbSet<Sensor> Sensores => Set<Sensor>();
    public DbSet<Leitura> Leituras => Set<Leitura>();
    public DbSet<Alerta> Alertas => Set<Alerta>();
    public DbSet<Relato> Relatos => Set<Relato>();

    public Task<int> SalvarAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AlagaAlertaDbContext).Assembly);    }
}
