using BuildingBlocks.Exceptions;

namespace Order.Command.Application.Exceptions;

public class OrderNotFoundExceptions(Guid id) : NotFoundException("Order", id);