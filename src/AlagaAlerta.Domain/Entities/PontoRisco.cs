using AlagaAlerta.Domain.ValueObjects;

namespace AlagaAlerta.Domain.Entities;

public class PontoRisco : Entidade
{
    private PontoRisco()
    {
    }

    public PontoRisco(string nome, string? descricao, double latitude, double longitude, LimitesAlagamento limites, Guid bairroId)
    {
        DefinirNome(nome);
        Descricao = Guarda.TextoOpcional(descricao, "descricao", 500);
        DefinirCoordenadas(latitude, longitude);
        Limites = limites ?? throw new ArgumentNullException(nameof(limites));
        BairroId = Guarda.Id(bairroId, "bairroId");
        CriadoEm = DateTime.UtcNow;
        AtualizadoEm = CriadoEm;
    }

    public string Nome { get; private set; } = string.Empty;
    public string? Descricao { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public LimitesAlagamento Limites { get; private set; } = null!;
    public Guid BairroId { get; private set; }
    public Bairro? Bairro { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public DateTime AtualizadoEm { get; private set; }

    public NivelRisco Classificar(double nivelAguaCm) => Limites.Classificar(nivelAguaCm);

    public void Atualizar(
        string? nome = null,
        string? descricao = null,
        double? latitude = null,
        double? longitude = null,
        double? nivelAtencaoCm = null,
        double? nivelCriticoCm = null,
        Guid? bairroId = null)
    {
        var novosLimites = Limites.Com(nivelAtencaoCm, nivelCriticoCm);

        if (nome is not null)
        {
            DefinirNome(nome);
        }
        if (descricao is not null)
        {
            Descricao = Guarda.TextoOpcional(descricao, "descricao", 500);
        }
        DefinirCoordenadas(latitude ?? Latitude, longitude ?? Longitude);
        if (novosLimites != Limites)
        {
            Limites = novosLimites;
        }
        if (bairroId.HasValue)
        {
            BairroId = Guarda.Id(bairroId.Value, "bairroId");
        }
        AtualizadoEm = DateTime.UtcNow;
    }

    private void DefinirNome(string nome) => Nome = Guarda.Texto(nome, "nome", 3, 120);

    private void DefinirCoordenadas(double latitude, double longitude)
    {
        Latitude = Guarda.Faixa(latitude, "latitude", -90, 90);
        Longitude = Guarda.Faixa(longitude, "longitude", -180, 180);
    }
}
