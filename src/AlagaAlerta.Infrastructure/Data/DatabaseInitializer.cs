using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AlagaAlerta.Infrastructure.Data;

public static class DatabaseInitializer
{
    public static async Task InicializarAsync(IServiceProvider services, bool popular, CancellationToken ct = default)
    {
        await using var escopo = services.CreateAsyncScope();
        var db = escopo.ServiceProvider.GetRequiredService<AlagaAlertaDbContext>();
        var logger = escopo.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("AlagaAlerta.Database");

        if (db.Database.IsRelational())
        {
            await db.Database.MigrateAsync(ct);
            logger.LogInformation("Migrações aplicadas.");
        }
        else
        {
            await db.Database.EnsureCreatedAsync(ct);
        }

        if (popular && !await db.Bairros.AnyAsync(ct))
        {
            await PopularAsync(db, ct);
            logger.LogInformation("Dados de exemplo inseridos.");
        }
    }

    public static async Task PopularAsync(AlagaAlertaDbContext db, CancellationToken ct = default)
    {
        var pompeia = new Bairro("Pompeia", Zona.Oeste);
        var se = new Bairro("Sé", Zona.Centro);
        var aricanduva = new Bairro("Aricanduva", Zona.Leste);
        db.Bairros.AddRange(pompeia, se, aricanduva);

        var pontoPompeia = new PontoRisco(
            "Av. Pompeia x R. Turiassu",
            "Baixada que recebe água do córrego canalizado em chuvas fortes",
            -23.5305, -46.6835, new LimitesAlagamento(15, 30), pompeia.Id);
        var pontoBandeira = new PontoRisco(
            "Praça da Bandeira",
            "Ponto de acúmulo de água no centro",
            -23.5489, -46.6388, new LimitesAlagamento(10, 25), se.Id);
        var pontoAricanduva = new PontoRisco(
            "Av. Aricanduva (trecho do córrego)",
            "Córrego Aricanduva transborda em chuvas intensas",
            -23.564, -46.511, new LimitesAlagamento(20, 40), aricanduva.Id);
        db.PontosRisco.AddRange(pontoPompeia, pontoBandeira, pontoAricanduva);

        var sensorPompeia = new Sensor("ESP32-POMPEIA-01", pontoPompeia.Id);
        db.Sensores.AddRange(
            sensorPompeia,
            new Sensor("ESP32-BANDEIRA-01", pontoBandeira.Id),
            new Sensor("ESP32-ARICANDUVA-01", pontoAricanduva.Id));

        var agora = DateTime.UtcNow;
        var historico = new (int MinutosAtras, double NivelCm, double ChuvaMmHora)[]
        {
            (60, 2, 0),
            (45, 6, 8),
            (30, 11, 22),
            (15, 9, 15)
        };
        db.Leituras.AddRange(historico.Select(h =>
            sensorPompeia.RegistrarLeitura(h.NivelCm, h.ChuvaMmHora, agora.AddMinutes(-h.MinutosAtras))));

        db.Relatos.Add(new Relato(
            aricanduva.Id,
            pontoAricanduva.Id,
            "Água chegando na calçada perto do ponto de ônibus",
            IntensidadeRelato.Media,
            null,
            null));

        await db.SaveChangesAsync(ct);
    }
}
