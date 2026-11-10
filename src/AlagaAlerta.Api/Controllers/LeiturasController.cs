using AlagaAlerta.Api.Erros;
using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AlagaAlerta.Api.Controllers;

[ApiController]
[Route("api/v1/leituras")]
[Produces("application/json")]
[SwaggerTag("Medições enviadas pelo ESP32")]
public sealed class LeiturasController : ControllerBase
{
    private readonly ILeituraService _service;

    public LeiturasController(ILeituraService service) => _service = service;

    [HttpPost]
    [SwaggerOperation(Summary = "Recebe a medição do sensor e abre, escala, mantém ou encerra o alerta do ponto")]
    [ProducesResponseType<ResultadoLeituraResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ErroResponse>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult<ResultadoLeituraResponse>> Registrar(RegistrarLeituraRequest request, CancellationToken ct)
    {
        var resultado = await _service.RegistrarAsync(request, ct);
        return StatusCode(StatusCodes.Status201Created, resultado);
    }
}
