using AlagaAlerta.Domain.Exceptions;

namespace AlagaAlerta.Domain;

public static class NivelRiscoExtensions
{
    public static bool GeraAlerta(this NivelRisco nivel) => nivel is NivelRisco.Atencao or NivelRisco.Critico;

    public static NivelAlerta ParaNivelAlerta(this NivelRisco nivel) => nivel switch
    {
        NivelRisco.Atencao => NivelAlerta.Atencao,
        NivelRisco.Critico => NivelAlerta.Critico,
        _ => throw new DomainException($"O nível {nivel} não gera alerta.")
    };
}
