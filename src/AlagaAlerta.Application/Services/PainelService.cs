using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;

namespace AlagaAlerta.Application.Services;

public sealed class PainelService : IPainelService
{
    private static readonly TimeSpan JanelaDuracao = TimeSpan.FromDays(30);

    private readonly IPainelConsultas _consultas;
    private readonly TimeProvider _relogio;

    public PainelService(IPainelConsultas consultas, TimeProvider relogio)
    {
        _consultas = consultas;
        _relogio = relogio;
    }

    public async Task<ResumoPainel> GerarAsync(int topPontos = 5, CancellationToken ct = default)
    {
        var agora = _relogio.GetUtcNow().UtcDateTime;

        var bairros = await _consultas.IndicadoresPorBairroAsync(ct);
        var pontos = await _consultas.PontosMaisAlertadosAsync(Math.Clamp(topPontos, 1, 50), ct);
        var duracoes = await _consultas.DuracoesAlertasEncerradosAsync(agora - JanelaDuracao, ct);

        double? duracaoMedia = duracoes.Count == 0
            ? null
            : Math.Round(duracoes.Average(d => d.TotalMinutes), 1);

        var ordenados = bairros
            .OrderByDescending(b => b.AlertasAtivos)
            .ThenByDescending(b => b.RelatosConfirmados)
            .ThenBy(b => b.Bairro)
            .ToList();

        return new ResumoPainel(
            agora,
            bairros.Count,
            bairros.Sum(b => b.PontosRisco),
            bairros.Sum(b => b.SensoresAtivos),
            bairros.Sum(b => b.AlertasAtivos),
            bairros.Sum(b => b.RelatosAbertos),
            duracaoMedia,
            ordenados,
            pontos);
    }
}
