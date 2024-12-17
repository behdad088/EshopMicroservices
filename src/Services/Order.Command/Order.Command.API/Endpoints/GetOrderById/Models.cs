using Microsoft.AspNetCore.Mvc;
using Order.Command.Application.Orders.Queries.GetOrderById;

namespace Order.Command.API.Endpoints.GetOrderById;

public record Request
{
    [FromRoute(Name = "id")] public string? Id { get; set; }
}

public record Response(GetOrderByIdDto Order);