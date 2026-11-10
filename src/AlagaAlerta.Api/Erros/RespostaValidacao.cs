using Microsoft.AspNetCore.Mvc;

namespace AlagaAlerta.Api.Erros;

public static class RespostaValidacao
{
    public static IActionResult Criar(ActionContext contexto)
    {
        var detalhes = contexto.ModelState
            .Where(e => e.Value is { Errors.Count: > 0 })
            .SelectMany(e => e.Value!.Errors.Select(erro => new DetalheErro(
                NomeDoCampo(e.Key),
                string.IsNullOrWhiteSpace(erro.ErrorMessage) ? "Valor inválido." : erro.ErrorMessage)))
            .ToList();

        return new BadRequestObjectResult(new ErroResponse("Dados inválidos", detalhes));
    }

    private static string NomeDoCampo(string chave)
    {
        var campo = chave.StartsWith("$.") ? chave[2..] : chave;
        if (string.IsNullOrEmpty(campo) || campo == "request")
        {
            return "corpo";
        }
        return char.ToLowerInvariant(campo[0]) + campo[1..];
    }
}
