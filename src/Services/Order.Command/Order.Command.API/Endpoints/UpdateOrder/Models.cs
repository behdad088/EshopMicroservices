using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.UpdateOrder;

public record Request(OrderDto OrderDto);
public record Response(bool Success);