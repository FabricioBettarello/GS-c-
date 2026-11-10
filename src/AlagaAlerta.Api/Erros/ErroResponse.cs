namespace AlagaAlerta.Api.Erros;

public sealed record DetalheErro(string Campo, string Mensagem);

public sealed record ErroResponse(string Erro, IReadOnlyList<DetalheErro>? Detalhes = null);
