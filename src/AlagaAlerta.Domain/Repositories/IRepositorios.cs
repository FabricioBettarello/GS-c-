using AlagaAlerta.Domain.Entities;

namespace AlagaAlerta.Domain.Repositories;

public interface IRepositorio<T> where T : Entidade
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken ct = default);
    Task AdicionarAsync(T entidade, CancellationToken ct = default);
    void Remover(T entidade);
}

public interface IBairroRepository : IRepositorio<Bairro>
{
    Task<bool> ExisteComNomeAsync(string nome, CancellationToken ct = default);
    Task<IReadOnlyList<Bairro>> ListarAsync(CancellationToken ct = default);
}

public interface IPontoRiscoRepository : IRepositorio<PontoRisco>
{
    Task<IReadOnlyList<PontoRisco>> ListarAsync(Guid? bairroId, CancellationToken ct = default);
}

public interface ISensorRepository : IRepositorio<Sensor>
{
    Task<Sensor?> ObterPorCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Sensor>> ListarAsync(Guid? pontoRiscoId, CancellationToken ct = default);
}

public interface ILeituraRepository : IRepositorio<Leitura>
{
    Task<IReadOnlyList<Leitura>> ListarPorSensorAsync(Guid sensorId, int limite, CancellationToken ct = default);
    Task<Leitura?> ObterUltimaDoPontoAsync(Guid pontoRiscoId, CancellationToken ct = default);
}

public interface IAlertaRepository : IRepositorio<Alerta>
{
    Task<Alerta?> ObterAtivoDoPontoAsync(Guid pontoRiscoId, CancellationToken ct = default);
    Task<IReadOnlyList<Alerta>> ListarAsync(bool? ativos, Guid? bairroId, CancellationToken ct = default);
}

public interface IRelatoRepository : IRepositorio<Relato>
{
    Task<IReadOnlyList<Relato>> ListarAsync(Guid? bairroId, StatusRelato? status, CancellationToken ct = default);
}

public interface IUnitOfWork
{
    Task<int> SalvarAsync(CancellationToken ct = default);
}
