using System.ComponentModel.DataAnnotations;
using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/painel")]
[Produces("application/json")]
[SwaggerTag("Indicadores de impacto por bairro")]
public sealed class PainelController : ControllerBase
{
    private readonly IPainelService _service;

    public PainelController(IPainelService service) => _service = service;

    [HttpGet]
    [SwaggerOperation(Summary = "Resumo da cidade: alertas ativos, relatos, pontos mais alertados e duração média dos alertas")]
    public async Task<ResumoPainel> Obter([FromQuery, Range(1, 50)] int top = 5, CancellationToken ct = default) =>
        await _service.GerarAsync(top, ct);
}
