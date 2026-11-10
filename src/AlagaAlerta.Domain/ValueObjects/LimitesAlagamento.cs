using AlagaAlerta.Domain.Exceptions;

namespace AlagaAlerta.Domain.ValueObjects;

public sealed record LimitesAlagamento
{
    public LimitesAlagamento(double nivelAtencaoCm, double nivelCriticoCm)
    {
        if (nivelAtencaoCm <= 0)
        {
            throw new DomainException("O nível de atenção deve ser maior que zero.");
        }
        if (nivelCriticoCm <= nivelAtencaoCm)
        {
            throw new DomainException("O nível crítico deve ser maior que o nível de atenção.");
        }

        NivelAtencaoCm = nivelAtencaoCm;
        NivelCriticoCm = nivelCriticoCm;
    }

    public double NivelAtencaoCm { get; }
    public double NivelCriticoCm { get; }

    public NivelRisco Classificar(double nivelAguaCm)
    {
        if (nivelAguaCm >= NivelCriticoCm)
        {
            return NivelRisco.Critico;
        }
        return nivelAguaCm >= NivelAtencaoCm ? NivelRisco.Atencao : NivelRisco.Normal;
    }

    public LimitesAlagamento Com(double? nivelAtencaoCm, double? nivelCriticoCm) =>
        new(nivelAtencaoCm ?? NivelAtencaoCm, nivelCriticoCm ?? NivelCriticoCm);
}
