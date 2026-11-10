using AlagaAlerta.Application.Abstractions;
using AlagaAlerta.Application.Dtos;
using AlagaAlerta.Domain.Entities;
using AlagaAlerta.Domain.Exceptions;
using AlagaAlerta.Domain.Repositories;

namespace AlagaAlerta.Application.Services;

public sealed class BairroService : IBairroService
{
    private readonly IBairroRepository _bairros;
    private readonly IUnitOfWork _uow;

    public BairroService(IBairroRepository bairros, IUnitOfWork uow)
    {
        _bairros = bairros;
        _uow = uow;
    }

    public async Task<BairroResponse> CriarAsync(CriarBairroRequest request, CancellationToken ct = default)
    {
        var nome = request.Nome.Trim();
        if (await _bairros.ExisteComNomeAsync(nome, ct))
        {
            throw new ConflictException($"O bairro \"{nome}\" já está cadastrado.");
        }

        var bairro = new Bairro(nome, request.Zona!.Value);
        await _bairros.AdicionarAsync(bairro, ct);
        await _uow.SalvarAsync(ct);
        return BairroResponse.De(bairro);
    }

    public async Task<IReadOnlyList<BairroResponse>> ListarAsync(CancellationToken ct = default)
    {
        var bairros = await _bairros.ListarAsync(ct);
        return bairros.Select(BairroResponse.De).ToList();
    }

    public async Task<BairroResponse> BuscarAsync(Guid id, CancellationToken ct = default)
    {
        var bairro = await _bairros.ObterPorIdAsync(id, ct) ?? throw new NotFoundException("Bairro", id);
        return BairroResponse.De(bairro);
    }
}
