using FastEndpoints;

namespace Order.Query.API.Features.GetOrderById;

public record Request(
    [property: BindFrom("CustomerId")]
    string? CustomerId,
    [property: BindFrom("OrderId")]
    string? OrderId);
public record Response(string OrderId);