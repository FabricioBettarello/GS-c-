using AlagaAlerta.Domain.Exceptions;

namespace AlagaAlerta.Domain.Entities;

public class Relato : Entidade
{
    private Relato()
    {
    }

    public Relato(Guid bairroId, Guid? pontoRiscoId, string descricao, IntensidadeRelato intensidade, double? latitude, double? longitude)
    {
        BairroId = Guarda.Id(bairroId, "bairroId");
        PontoRiscoId = pontoRiscoId;
        Descricao = Guarda.Texto(descricao, "descricao", 5, 500);
        Intensidade = intensidade;
        Latitude = latitude.HasValue ? Guarda.Faixa(latitude.Value, "latitude", -90, 90) : null;
        Longitude = longitude.HasValue ? Guarda.Faixa(longitude.Value, "longitude", -180, 180) : null;
        Status = StatusRelato.Aberto;
        CriadoEm = DateTime.UtcNow;
    }

    public string Descricao { get; private set; } = string.Empty;
    public IntensidadeRelato Intensidade { get; private set; }
    public StatusRelato Status { get; private set; }
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public Guid BairroId { get; private set; }
    public Bairro? Bairro { get; private set; }
    public Guid? PontoRiscoId { get; private set; }
    public PontoRisco? PontoRisco { get; private set; }

    public void Moderar(StatusRelato novoStatus)
    {
        if (novoStatus == StatusRelato.Aberto)
        {
            throw new DomainException("Um relato só pode ser confirmado ou descartado.");
        }
        if (Status != StatusRelato.Aberto)
        {
            throw new DomainException($"Este relato já foi {Status.ToString().ToLowerInvariant()} e não pode mudar.");
        }
        Status = novoStatus;
    }
}
