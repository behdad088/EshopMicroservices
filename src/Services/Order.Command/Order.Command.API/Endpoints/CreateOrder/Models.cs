using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.CreateOrder;

public record Request(OrderDto OrderDto);
public record Response(Guid OrderId);