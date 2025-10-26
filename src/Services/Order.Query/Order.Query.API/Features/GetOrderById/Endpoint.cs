using FastEndpoints;

namespace Order.Query.API.Features.GetOrderById;

public class Endpoint : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("/customer/{CustomerId}/orders/{OrderId}");
        AllowAnonymous();
        DontCatchExceptions();
        Version(1);
    }

    public override async Task HandleAsync(Request req, CancellationToken ct)
    {
        await Send.OkAsync(ct); 
        return;
    }
}