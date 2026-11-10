using AlagaAlerta.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AlagaAlerta.Api.Erros;

public sealed class TratadorDeErros : IExceptionHandler
{
    private readonly ILogger<TratadorDeErros> _logger;

    public TratadorDeErros(ILogger<TratadorDeErros> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception exception, CancellationToken ct)
    {
        var (status, mensagem) = exception switch
        {
            DomainException e => (e.StatusCode, e.Message),
            DbUpdateException => (StatusCodes.Status409Conflict, "Não foi possível salvar: registro duplicado ou em uso."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Requisição inválida."),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno do servidor.")
        };

        if (status >= 500)
        {
            _logger.LogError(exception, "Erro não tratado em {Metodo} {Caminho}", http.Request.Method, http.Request.Path);
        }

        http.Response.StatusCode = status;
        await http.Response.WriteAsJsonAsync(new ErroResponse(mensagem), ct);
        return true;
    }
}
