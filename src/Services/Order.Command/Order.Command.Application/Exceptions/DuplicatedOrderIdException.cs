using BuildingBlocks.Exceptions;

namespace Order.Command.Application.Exceptions;

public class DuplicatedOrderIdException : BadRequestException
{
    public DuplicatedOrderIdException(string message) : base(message)
    {
    }

    public DuplicatedOrderIdException(string message, string details) : base(message, details)
    {
    }
}