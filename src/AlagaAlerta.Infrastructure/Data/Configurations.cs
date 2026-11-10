using AlagaAlerta.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlagaAlerta.Infrastructure.Data;

internal sealed class BairroConfiguration : IEntityTypeConfiguration<Bairro>
{
    public void Configure(EntityTypeBuilder<Bairro> builder)
    {
        builder.ToTable("bairros");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Nome).HasMaxLength(80).IsRequired();
        builder.Property(b => b.Zona).HasConversion<string>().HasMaxLength(10);
        builder.HasIndex(b => b.Nome).IsUnique();

        builder.HasMany(b => b.Pontos)
            .WithOne(p => p.Bairro)
            .HasForeignKey(p => p.BairroId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.Navigation(b => b.Pontos).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}

internal sealed class PontoRiscoConfiguration : IEntityTypeConfiguration<PontoRisco>
{
    public void Configure(EntityTypeBuilder<PontoRisco> builder)
    {
        builder.ToTable("pontos_risco");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).HasMaxLength(120).IsRequired();
        builder.Property(p => p.Descricao).HasMaxLength(500);

        builder.OwnsOne(p => p.Limites, limites =>
        {
            limites.Property(l => l.NivelAtencaoCm).HasColumnName("nivel_atencao_cm");
            limites.Property(l => l.NivelCriticoCm).HasColumnName("nivel_critico_cm");
        });
        builder.Navigation(p => p.Limites).IsRequired();

        builder.HasIndex(p => p.BairroId);
    }
}

internal sealed class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.ToTable("sensores");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Codigo).HasMaxLength(40).IsRequired();
        builder.HasIndex(s => s.Codigo).IsUnique();
        builder.HasIndex(s => s.PontoRiscoId);

        builder.HasOne(s => s.PontoRisco)
            .WithMany()
            .HasForeignKey(s => s.PontoRiscoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LeituraConfiguration : IEntityTypeConfiguration<Leitura>
{
    public void Configure(EntityTypeBuilder<Leitura> builder)
    {
        builder.ToTable("leituras");
        builder.HasKey(l => l.Id);
        builder.HasIndex(l => new { l.SensorId, l.RegistradaEm });

        builder.HasOne(l => l.Sensor)
            .WithMany()
            .HasForeignKey(l => l.SensorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AlertaConfiguration : IEntityTypeConfiguration<Alerta>
{
    public void Configure(EntityTypeBuilder<Alerta> builder)
    {
        builder.ToTable("alertas");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Nivel).HasConversion<string>().HasMaxLength(10);
        builder.Property(a => a.Mensagem).HasMaxLength(300).IsRequired();
        builder.Ignore(a => a.Duracao);
        builder.HasIndex(a => new { a.PontoRiscoId, a.Ativo });

        builder.HasOne(a => a.PontoRisco)
            .WithMany()
            .HasForeignKey(a => a.PontoRiscoId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class RelatoConfiguration : IEntityTypeConfiguration<Relato>
{
    public void Configure(EntityTypeBuilder<Relato> builder)
    {
        builder.ToTable("relatos");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Descricao).HasMaxLength(500).IsRequired();
        builder.Property(r => r.Intensidade).HasConversion<string>().HasMaxLength(15);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(12);
        builder.HasIndex(r => new { r.BairroId, r.Status });

        builder.HasOne(r => r.Bairro)
            .WithMany()
            .HasForeignKey(r => r.BairroId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.PontoRisco)
            .WithMany()
            .HasForeignKey(r => r.PontoRiscoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
