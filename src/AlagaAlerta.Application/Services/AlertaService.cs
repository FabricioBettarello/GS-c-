using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain.Repositories;

namespace AlagaAlerta.Application.Services;

public sealed class AlertaService : IAlertaService
{
    private readonly IAlertaRepository _alertas;

    public AlertaService(IAlertaRepository alertas) => _alertas = alertas;

    public async Task<IReadOnlyList<AlertaResponse>> ListarAsync(bool? ativos, Guid? bairroId, CancellationToken ct = default)
    {
        var alertas = await _alertas.ListarAsync(ativos, bairroId, ct);
        return alertas.Select(AlertaResponse.De).ToList();
    }
}
