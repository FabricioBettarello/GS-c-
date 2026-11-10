namespace AlagaAlerta.Domain.Exceptions;

public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public virtual int StatusCode => 422;
}

public class NotFoundException : DomainException
{
    public NotFoundException(string recurso, object id)
        : base($"{recurso} não encontrado(a): {id}")
    {
    }

    public override int StatusCode => 404;
}

public class ConflictException : DomainException
{
    public ConflictException(string message) : base(message)
    {
    }

    public override int StatusCode => 409;
}
