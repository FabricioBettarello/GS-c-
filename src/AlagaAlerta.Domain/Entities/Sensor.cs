using System.Text.RegularExpressions;
using AlagaAlerta.Domain.Exceptions;

namespace AlagaAlerta.Domain.Entities;

public partial class Sensor : Entidade
{
    private Sensor()
    {
    }

    public Sensor(string codigo, Guid pontoRiscoId, bool ativo = true)
    {
        Codigo = NormalizarCodigo(codigo);
        if (!FormatoCodigo().IsMatch(Codigo))
        {
            throw new DomainException("O código do sensor deve ter de 3 a 40 caracteres entre letras, números e hífen.");
        }
        PontoRiscoId = Guarda.Id(pontoRiscoId, "pontoRiscoId");
        Ativo = ativo;
        CriadoEm = DateTime.UtcNow;
    }

    public string Codigo { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public Guid PontoRiscoId { get; private set; }
    public PontoRisco? PontoRisco { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public static string NormalizarCodigo(string? codigo) => (codigo ?? string.Empty).Trim().ToUpperInvariant();

    public void Ativar() => Ativo = true;

    public void Desativar() => Ativo = false;

    public Leitura RegistrarLeitura(double nivelAguaCm, double? chuvaMmHora, DateTime registradaEm)
    {
        if (!Ativo)
        {
            throw new DomainException($"O sensor {Codigo} está inativo.");
        }
        return new Leitura(Id, nivelAguaCm, chuvaMmHora, registradaEm);
    }

    [GeneratedRegex("^[A-Z0-9-]{3,40}$")]
    private static partial Regex FormatoCodigo();
}
