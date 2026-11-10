using AlagaAlerta.Domain;
using AlagaAlerta.Domain.Entities;

namespace AlagaAlerta.Application.Dtos;

public sealed record BairroResponse(Guid Id, string Nome, Zona Zona, DateTime CriadoEm)
{
    public static BairroResponse De(Bairro b) => new(b.Id, b.Nome, b.Zona, b.CriadoEm);
}

public sealed record PontoRiscoResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    double Latitude,
    double Longitude,
    double NivelAtencaoCm,
    double NivelCriticoCm,
    Guid BairroId,
    DateTime CriadoEm,
    DateTime AtualizadoEm)
{
    public static PontoRiscoResponse De(PontoRisco p) => new(
        p.Id, p.Nome, p.Descricao, p.Latitude, p.Longitude,
        p.Limites.NivelAtencaoCm, p.Limites.NivelCriticoCm,
        p.BairroId, p.CriadoEm, p.AtualizadoEm);
}

public sealed record SensorResponse(Guid Id, string Codigo, bool Ativo, Guid PontoRiscoId, DateTime CriadoEm)
{
    public static SensorResponse De(Sensor s) => new(s.Id, s.Codigo, s.Ativo, s.PontoRiscoId, s.CriadoEm);
}

public sealed record LeituraResponse(Guid Id, double NivelAguaCm, double? ChuvaMmHora, DateTime RegistradaEm, Guid SensorId)
{
    public static LeituraResponse De(Leitura l) => new(l.Id, l.NivelAguaCm, l.ChuvaMmHora, l.RegistradaEm, l.SensorId);
}

public sealed record AlertaResponse(
    Guid Id,
    NivelAlerta Nivel,
    string Mensagem,
    bool Ativo,
    DateTime CriadoEm,
    DateTime? EncerradoEm,
    Guid PontoRiscoId)
{
    public static AlertaResponse De(Alerta a) =>
        new(a.Id, a.Nivel, a.Mensagem, a.Ativo, a.CriadoEm, a.EncerradoEm, a.PontoRiscoId);
}

public sealed record RelatoResponse(
    Guid Id,
    string Descricao,
    IntensidadeRelato Intensidade,
    StatusRelato Status,
    double? Latitude,
    double? Longitude,
    DateTime CriadoEm,
    Guid BairroId,
    Guid? PontoRiscoId)
{
    public static RelatoResponse De(Relato r) => new(
        r.Id, r.Descricao, r.Intensidade, r.Status, r.Latitude, r.Longitude,
        r.CriadoEm, r.BairroId, r.PontoRiscoId);
}

public sealed record StatusPontoResponse(
    PontoRiscoResponse Ponto,
    NivelRisco NivelAtual,
    LeituraResponse? UltimaLeitura,
    AlertaResponse? AlertaAtivo);

public sealed record ResultadoLeituraResponse(
    LeituraResponse Leitura,
    NivelRisco Nivel,
    AcaoAlerta Acao,
    AlertaResponse? Alerta);

public sealed record IndicadoresBairro(
    Guid BairroId,
    string Bairro,
    Zona Zona,
    int PontosRisco,
    int SensoresAtivos,
    int AlertasAtivos,
    int AlertasEmitidos,
    int RelatosConfirmados,
    int RelatosAbertos);

public sealed record PontoMaisAlertado(
    Guid PontoRiscoId,
    string Ponto,
    string Bairro,
    int TotalAlertas,
    int AlertasCriticos,
    DateTime? UltimoAlertaEm);

public sealed record ResumoPainel(
    DateTime GeradoEm,
    int TotalBairros,
    int TotalPontosRisco,
    int SensoresAtivos,
    int AlertasAtivos,
    int RelatosAbertos,
    double? DuracaoMediaAlertasMinutos,
    IReadOnlyList<IndicadoresBairro> Bairros,
    IReadOnlyList<PontoMaisAlertado> PontosMaisAlertados);
