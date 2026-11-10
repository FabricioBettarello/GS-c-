using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain;

namespace AlagaAlerta.Application.Abstractions;

public interface IBairroService
{
    Task<BairroResponse> CriarAsync(CriarBairroRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<BairroResponse>> ListarAsync(CancellationToken ct = default);
    Task<BairroResponse> BuscarAsync(Guid id, CancellationToken ct = default);
}

public interface IPontoRiscoService
{
    Task<PontoRiscoResponse> CriarAsync(CriarPontoRiscoRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<PontoRiscoResponse>> ListarAsync(Guid? bairroId, CancellationToken ct = default);
    Task<PontoRiscoResponse> BuscarAsync(Guid id, CancellationToken ct = default);
    Task<PontoRiscoResponse> AtualizarAsync(Guid id, AtualizarPontoRiscoRequest request, CancellationToken ct = default);
    Task RemoverAsync(Guid id, CancellationToken ct = default);
    Task<StatusPontoResponse> StatusAsync(Guid id, CancellationToken ct = default);
}

public interface ISensorService
{
    Task<SensorResponse> CriarAsync(CriarSensorRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<SensorResponse>> ListarAsync(Guid? pontoRiscoId, CancellationToken ct = default);
    Task<SensorResponse> AlterarAtivoAsync(Guid id, bool ativo, CancellationToken ct = default);
    Task<IReadOnlyList<LeituraResponse>> HistoricoAsync(Guid id, int limite, CancellationToken ct = default);
}

public interface ILeituraService
{
    Task<ResultadoLeituraResponse> RegistrarAsync(RegistrarLeituraRequest request, CancellationToken ct = default);
}

public interface IAlertaService
{
    Task<IReadOnlyList<AlertaResponse>> ListarAsync(bool? ativos, Guid? bairroId, CancellationToken ct = default);
}

public interface IRelatoService
{
    Task<RelatoResponse> CriarAsync(CriarRelatoRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<RelatoResponse>> ListarAsync(Guid? bairroId, StatusRelato? status, CancellationToken ct = default);
    Task<RelatoResponse> ModerarAsync(Guid id, StatusRelato status, CancellationToken ct = default);
}

public interface IPainelService
{
    Task<ResumoPainel> GerarAsync(int topPontos = 5, CancellationToken ct = default);
}

public interface IPainelConsultas
{
    Task<IReadOnlyList<IndicadoresBairro>> IndicadoresPorBairroAsync(CancellationToken ct = default);
    Task<IReadOnlyList<PontoMaisAlertado>> PontosMaisAlertadosAsync(int quantidade, CancellationToken ct = default);
    Task<IReadOnlyList<TimeSpan>> DuracoesAlertasEncerradosAsync(DateTime desde, CancellationToken ct = default);
}
