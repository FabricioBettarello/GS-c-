using System.ComponentModel.DataAnnotations;
using AlagaAlerta.Api.Erros;
using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/sensores")]
[Produces("application/json")]
[SwaggerTag("Sensores ESP32 instalados nos pontos de risco")]
public sealed class SensoresController : ControllerBase
{
    private readonly ISensorService _service;

    public SensoresController(ISensorService service) => _service = service;

    [HttpPost]
    [SwaggerOperation(Summary = "Cadastra um sensor em um ponto de risco")]
    [ProducesResponseType<SensorResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SensorResponse>> Criar(CriarSensorRequest request, CancellationToken ct)
    {
        var sensor = await _service.CriarAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, sensor);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lista sensores, com filtro opcional por ponto de risco")]
    public async Task<IReadOnlyList<SensorResponse>> Listar([FromQuery] Guid? pontoRiscoId, CancellationToken ct) =>
        await _service.ListarAsync(pontoRiscoId, ct);

    [HttpPatch("{id:guid}")]
    [SwaggerOperation(Summary = "Ativa ou desativa um sensor")]
    [ProducesResponseType<SensorResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SensorResponse>> AlterarAtivo(Guid id, AtualizarSensorRequest request, CancellationToken ct) =>
        await _service.AlterarAtivoAsync(id, request.Ativo!.Value, ct);

    [HttpGet("{id:guid}/leituras")]
    [SwaggerOperation(Summary = "Histórico de leituras do sensor, mais recentes primeiro")]
    [ProducesResponseType<IReadOnlyList<LeituraResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<LeituraResponse>>> Historico(
        Guid id,
        [FromQuery, Range(1, 500)] int limite = 50,
        CancellationToken ct = default) =>
        Ok(await _service.HistoricoAsync(id, limite, ct));
}
