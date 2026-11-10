namespace AlagaAlerta.Domain.Entities;

public class Bairro : Entidade
{
    private readonly List<PontoRisco> _pontos = new();

    private Bairro()
    {
    }

    public Bairro(string nome, Zona zona)
    {
        Nome = Guarda.Texto(nome, "nome", 2, 80);
        Zona = zona;
        CriadoEm = DateTime.UtcNow;
    }

    public string Nome { get; private set; } = string.Empty;
    public Zona Zona { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public IReadOnlyCollection<PontoRisco> Pontos => _pontos.AsReadOnly();
}
