using AlagaAlerta.Api.Erros;
using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/pontos-risco")]
[Produces("application/json")]
[SwaggerTag("Pontos da cidade que costumam alagar")]
public sealed class PontosRiscoController : ControllerBase
{
    private readonly IPontoRiscoService _service;

    public PontosRiscoController(IPontoRiscoService service) => _service = service;

    [HttpPost]
    [SwaggerOperation(Summary = "Cadastra um ponto de risco com os limites de atenção e crítico")]
    [ProducesResponseType<PontoRiscoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PontoRiscoResponse>> Criar(CriarPontoRiscoRequest request, CancellationToken ct)
    {
        var ponto = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Buscar), new { id = ponto.Id }, ponto);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lista pontos de risco, com filtro opcional por bairro")]
    public async Task<IReadOnlyList<PontoRiscoResponse>> Listar([FromQuery] Guid? bairroId, CancellationToken ct) =>
        await _service.ListarAsync(bairroId, ct);

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Busca um ponto de risco")]
    [ProducesResponseType<PontoRiscoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PontoRiscoResponse>> Buscar(Guid id, CancellationToken ct) =>
        await _service.BuscarAsync(id, ct);

    [HttpPatch("{id:guid}")]
    [SwaggerOperation(Summary = "Atualiza dados ou limites do ponto")]
    [ProducesResponseType<PontoRiscoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<PontoRiscoResponse>> Atualizar(Guid id, AtualizarPontoRiscoRequest request, CancellationToken ct) =>
        await _service.AtualizarAsync(id, request, ct);

    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Remove um ponto de risco e seus sensores")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken ct)
    {
        await _service.RemoverAsync(id, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/status")]
    [SwaggerOperation(Summary = "Situação atual do ponto: última leitura, nível e alerta ativo")]
    [ProducesResponseType<StatusPontoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StatusPontoResponse>> Status(Guid id, CancellationToken ct) =>
        await _service.StatusAsync(id, ct);
}
