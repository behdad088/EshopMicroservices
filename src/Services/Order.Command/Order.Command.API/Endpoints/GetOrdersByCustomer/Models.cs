using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.GetOrdersByCustomer;

public record Request
{
    [property: FromQuery(Name = "customer_id")]
    public string CustomerId { get; set; }
}

public record Response([property: JsonPropertyName("order")] IEnumerable<OrderDto> Orders);