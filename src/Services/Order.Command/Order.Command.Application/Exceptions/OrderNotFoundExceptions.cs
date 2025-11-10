using eshop.Shared.Exceptions;

namespace Order.Command.Application.Exceptions;

public class OrderNotFoundExceptions(Ulid id) : NotFoundException("Order", id);