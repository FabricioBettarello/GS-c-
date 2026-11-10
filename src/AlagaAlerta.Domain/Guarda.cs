using AlagaAlerta.Domain.Exceptions;

namespace AlagaAlerta.Domain;

internal static class Guarda
{
    public static string Texto(string? valor, string campo, int minimo, int maximo)
    {
        var texto = valor?.Trim() ?? string.Empty;
        if (texto.Length < minimo || texto.Length > maximo)
        {
            throw new DomainException($"O campo {campo} deve ter entre {minimo} e {maximo} caracteres.");
        }
        return texto;
    }

    public static string? TextoOpcional(string? valor, string campo, int maximo)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }
        var texto = valor.Trim();
        if (texto.Length > maximo)
        {
            throw new DomainException($"O campo {campo} deve ter no máximo {maximo} caracteres.");
        }
        return texto;
    }

    public static double Faixa(double valor, string campo, double minimo, double maximo)
    {
        if (double.IsNaN(valor) || valor < minimo || valor > maximo)
        {
            throw new DomainException($"O campo {campo} deve estar entre {minimo} e {maximo}.");
        }
        return valor;
    }

    public static Guid Id(Guid valor, string campo)
    {
        if (valor == Guid.Empty)
        {
            throw new DomainException($"O campo {campo} é obrigatório.");
        }
        return valor;
    }
}
