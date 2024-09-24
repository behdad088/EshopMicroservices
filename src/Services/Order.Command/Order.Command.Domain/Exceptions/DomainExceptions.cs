namespace Order.Command.Domain.Exceptions;

public class DomainExceptions : Exception
{
    public DomainExceptions(string message) : base($"Domain exceptions: {message} throws from domain layer.")
    {
    }
}