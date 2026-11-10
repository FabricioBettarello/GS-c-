using AlagaAlerta.Domain.Exceptions;

namespace AlagaAlerta.Domain.Entities;

public class Alerta : Entidade
{
    private Alerta()
    {
    }

    private Alerta(Guid pontoRiscoId, NivelAlerta nivel, string mensagem, DateTime criadoEm)
    {
        PontoRiscoId = pontoRiscoId;
        Nivel = nivel;
        Mensagem = mensagem;
        Ativo = true;
        CriadoEm = criadoEm;
    }

    public NivelAlerta Nivel { get; private set; }
    public string Mensagem { get; private set; } = string.Empty;
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime? EncerradoEm { get; private set; }
    public Guid PontoRiscoId { get; private set; }
    public PontoRisco? PontoRisco { get; private set; }

    public TimeSpan? Duracao => EncerradoEm - CriadoEm;

    public static Alerta Emitir(PontoRisco ponto, NivelAlerta nivel, double nivelAguaCm, DateTime agora)
    {
        ArgumentNullException.ThrowIfNull(ponto);
        return new Alerta(ponto.Id, nivel, MontarMensagem(nivel, ponto.Nome, nivelAguaCm), agora);
    }

    public void Encerrar(DateTime em)
    {
        if (!Ativo)
        {
            throw new DomainException("Este alerta já foi encerrado.");
        }
        Ativo = false;
        EncerradoEm = em;
    }

    public static string MontarMensagem(NivelAlerta nivel, string nomePonto, double nivelAguaCm)
    {
        var cm = (int)Math.Round(nivelAguaCm, MidpointRounding.AwayFromZero);
        return nivel == NivelAlerta.Critico
            ? $"Alagamento em {nomePonto}. A água está com {cm} cm. Evite passar pelo local."
            : $"Atenção: a água está subindo em {nomePonto} ({cm} cm). Se puder, use outro caminho.";
    }
}
