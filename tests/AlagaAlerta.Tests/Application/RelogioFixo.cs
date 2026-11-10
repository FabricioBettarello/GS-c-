namespace AlagaAlerta.Tests.Application;

internal sealed class RelogioFixo : TimeProvider
{
    public RelogioFixo(DateTimeOffset agora) => Agora = agora;

    public DateTimeOffset Agora { get; set; }

    public override DateTimeOffset GetUtcNow() => Agora;

    public void Avancar(TimeSpan tempo) => Agora += tempo;
}
