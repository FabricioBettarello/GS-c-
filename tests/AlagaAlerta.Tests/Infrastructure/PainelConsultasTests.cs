using AlagaAlerta.Application.Services;
using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Infrastructure.Data;
using AlagaAlerta.Infrastructure.Repositories;
using AlagaAlerta.Tests.Application;
using Microsoft.EntityFrameworkCore;

namespace AlagaAlerta.Tests.Infrastructure;

public class PainelConsultasTests : IAsyncLifetime
{
    private readonly AlagaAlertaDbContext _db = new(
        new DbContextOptionsBuilder<AlagaAlertaDbContext>()
            .UseInMemoryDatabase($"painel-{Guid.NewGuid()}")
            .Options);

    public async Task InitializeAsync() => await DatabaseInitializer.PopularAsync(_db);

    public async Task DisposeAsync() => await _db.DisposeAsync();

    private async Task<Alerta> EmitirAsync(string nomePonto, NivelAlerta nivel, DateTime em)
    {
        var ponto = await _db.PontosRisco.SingleAsync(p => p.Nome == nomePonto);
        var alerta = Alerta.Emitir(ponto, nivel, 30, em);
        _db.Alertas.Add(alerta);
        await _db.SaveChangesAsync();
        return alerta;
    }

    [Fact]
    public async Task Indicadores_por_bairro_contam_pontos_sensores_e_relatos()
    {
        var consultas = new PainelConsultas(_db);

        var indicadores = await consultas.IndicadoresPorBairroAsync();

        Assert.Equal(["Aricanduva", "Pompeia", "Sé"], indicadores.Select(i => i.Bairro));
        Assert.All(indicadores, i => Assert.Equal(1, i.PontosRisco));
        Assert.All(indicadores, i => Assert.Equal(1, i.SensoresAtivos));
        Assert.Equal(1, indicadores.Single(i => i.Bairro == "Aricanduva").RelatosAbertos);
    }

    [Fact]
    public async Task Pontos_mais_alertados_vem_ordenados_pelo_total()
    {
        var agora = DateTime.UtcNow;
        (await EmitirAsync("Praça da Bandeira", NivelAlerta.Atencao, agora.AddHours(-3))).Encerrar(agora.AddHours(-2));
        await EmitirAsync("Praça da Bandeira", NivelAlerta.Critico, agora.AddHours(-1));
        await EmitirAsync("Av. Pompeia x R. Turiassu", NivelAlerta.Atencao, agora);
        await _db.SaveChangesAsync();
        var consultas = new PainelConsultas(_db);

        var pontos = await consultas.PontosMaisAlertadosAsync(5);

        Assert.Equal(2, pontos.Count);
        Assert.Equal("Praça da Bandeira", pontos[0].Ponto);
        Assert.Equal("Sé", pontos[0].Bairro);
        Assert.Equal(2, pontos[0].TotalAlertas);
        Assert.Equal(1, pontos[0].AlertasCriticos);
    }

    [Fact]
    public async Task Resumo_soma_os_bairros_e_calcula_a_duracao_media()
    {
        var agora = new DateTime(2026, 11, 10, 18, 0, 0, DateTimeKind.Utc);
        (await EmitirAsync("Praça da Bandeira", NivelAlerta.Atencao, agora.AddMinutes(-90))).Encerrar(agora.AddMinutes(-60));
        (await EmitirAsync("Av. Pompeia x R. Turiassu", NivelAlerta.Critico, agora.AddMinutes(-50))).Encerrar(agora);
        await EmitirAsync("Av. Pompeia x R. Turiassu", NivelAlerta.Atencao, agora);
        await _db.SaveChangesAsync();
        var service = new PainelService(new PainelConsultas(_db), new RelogioFixo(agora));

        var resumo = await service.GerarAsync();

        Assert.Equal(3, resumo.TotalBairros);
        Assert.Equal(3, resumo.TotalPontosRisco);
        Assert.Equal(1, resumo.AlertasAtivos);
        Assert.Equal(40, resumo.DuracaoMediaAlertasMinutos);
        Assert.Equal("Pompeia", resumo.Bairros[0].Bairro);
    }
}
