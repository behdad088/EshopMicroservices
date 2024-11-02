using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.CreateOrder;

public record Request
{
    [property: JsonPropertyName("order")] public CreateOrderDto Order { get; set; }
}

public record Response(
    [property: JsonPropertyName("order_id")]
    Guid Id);