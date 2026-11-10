namespace AlagaAlerta.Domain.Entities;

public class Leitura : Entidade
{
    private Leitura()
    {
    }

    internal Leitura(Guid sensorId, double nivelAguaCm, double? chuvaMmHora, DateTime registradaEm)
    {
        SensorId = Guarda.Id(sensorId, "sensorId");
        NivelAguaCm = Guarda.Faixa(nivelAguaCm, "nivelAguaCm", 0, 1000);
        ChuvaMmHora = chuvaMmHora.HasValue ? Guarda.Faixa(chuvaMmHora.Value, "chuvaMmHora", 0, 500) : null;
        RegistradaEm = DateTime.SpecifyKind(registradaEm, DateTimeKind.Utc);
    }

    public double NivelAguaCm { get; private set; }
    public double? ChuvaMmHora { get; private set; }
    public DateTime RegistradaEm { get; private set; }
    public Guid SensorId { get; private set; }
    public Sensor? Sensor { get; private set; }
}
