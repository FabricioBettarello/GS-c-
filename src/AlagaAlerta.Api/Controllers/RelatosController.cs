using AlagaAlerta.Api.Erros;
using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/relatos")]
[Produces("application/json")]
[SwaggerTag("Relatos de alagamento enviados pelos cidadãos, sem dados pessoais")]
public sealed class RelatosController : ControllerBase
{
    private readonly IRelatoService _service;

    public RelatosController(IRelatoService service) => _service = service;

    [HttpPost]
    [SwaggerOperation(Summary = "Registra um relato de alagamento")]
    [ProducesResponseType<RelatoResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RelatoResponse>> Criar(CriarRelatoRequest request, CancellationToken ct)
    {
        var relato = await _service.CriarAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, relato);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lista relatos, com filtros por bairro e status")]
    public async Task<IReadOnlyList<RelatoResponse>> Listar([FromQuery] Guid? bairroId, [FromQuery] StatusRelato? status, CancellationToken ct) =>
        await _service.ListarAsync(bairroId, status, ct);

    [HttpPatch("{id:guid}/status")]
    [SwaggerOperation(Summary = "Confirma ou descarta um relato aberto")]
    [ProducesResponseType<RelatoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<RelatoResponse>> Moderar(Guid id, ModerarRelatoRequest request, CancellationToken ct) =>
        await _service.ModerarAsync(id, request.Status!.Value, ct);
}
