using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Dtos;

namespace Order.Command.API.Endpoints.UpdateOrder;

public record Request
{
    [FromBody] public UpdateOrderDto Order { get; set; }
}

public record Response(
    [property: JsonPropertyName("success")]
    bool IsSuccess);