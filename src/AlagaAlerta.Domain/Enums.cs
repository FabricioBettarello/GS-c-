namespace AlagaAlerta.Domain;

public enum Zona
{
    Centro,
    Norte,
    Sul,
    Leste,
    Oeste
}

public enum NivelAlerta
{
    Atencao,
    Critico
}

public enum NivelRisco
{
    SemDados,
    Normal,
    Atencao,
    Critico
}

public enum IntensidadeRelato
{
    Baixa,
    Media,
    Alta,
    Intransitavel
}

public enum StatusRelato
{
    Aberto,
    Confirmado,
    Descartado
}

public enum AcaoAlerta
{
    Nenhuma,
    Criado,
    Atualizado,
    Mantido,
    Encerrado
}
