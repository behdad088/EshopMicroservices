using Microsoft.AspNetCore.Mvc;

namespace Order.Command.API.Endpoints.DeleteOrder;

public record Request
{
    [FromRoute(Name = "order_id")] 
    public string? OrderId { get; set; }
}