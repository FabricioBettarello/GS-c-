using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/alertas")]
[Produces("application/json")]
[SwaggerTag("Alertas gerados a partir das leituras")]
public sealed class AlertasController : ControllerBase
{
    private readonly IAlertaService _service;

    public AlertasController(IAlertaService service) => _service = service;

    [HttpGet]
    [SwaggerOperation(Summary = "Lista alertas, com filtros por situação e bairro")]
    public async Task<IReadOnlyList<AlertaResponse>> Listar(
        [FromQuery, SwaggerParameter("true para listar só os alertas em andamento")] bool? ativos,
        [FromQuery] Guid? bairroId,
        CancellationToken ct) =>
        await _service.ListarAsync(ativos, bairroId, ct);
}
