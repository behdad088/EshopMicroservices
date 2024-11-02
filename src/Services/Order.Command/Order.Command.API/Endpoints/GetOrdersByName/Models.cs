using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.GetOrdersByName;

public record Request
{
    [FromQuery(Name = "name")] public string Name { get; set; }
}

public record Response([property: JsonPropertyName("orders")] IEnumerable<OrderDto> Orders);