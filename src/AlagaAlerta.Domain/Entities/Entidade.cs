namespace AlagaAlerta.Domain.Entities;

public abstract class Entidade
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public override bool Equals(object? obj) =>
        obj is Entidade outra && outra.GetType() == GetType() && outra.Id == Id;

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
