using AlagaAlerta.Api.Erros;
using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/bairros")]
[Produces("application/json")]
[SwaggerTag("Bairros monitorados")]
public sealed class BairrosController : ControllerBase
{
    private readonly IBairroService _service;

    public BairrosController(IBairroService service) => _service = service;

    [HttpPost]
    [SwaggerOperation(Summary = "Cadastra um bairro")]
    [ProducesResponseType<BairroResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BairroResponse>> Criar(CriarBairroRequest request, CancellationToken ct)
    {
        var bairro = await _service.CriarAsync(request, ct);
        return CreatedAtAction(nameof(Buscar), new { id = bairro.Id }, bairro);
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lista os bairros")]
    public async Task<IReadOnlyList<BairroResponse>> Listar(CancellationToken ct) =>
        await _service.ListarAsync(ct);

    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Busca um bairro")]
    [ProducesResponseType<BairroResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BairroResponse>> Buscar(Guid id, CancellationToken ct) =>
        await _service.BuscarAsync(id, ct);
}
