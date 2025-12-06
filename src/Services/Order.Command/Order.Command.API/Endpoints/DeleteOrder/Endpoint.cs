using System.Net;
using Order.Command.Application.Orders.Commands.DeleteOrder;

namespace Order.Command.API.Endpoints.DeleteOrder;

public class Endpoint : EndpointBase<Request>
{
    public override void MapEndpoint()
    {
        Delete("/customers/{customer_id}/orders/{order_id}", HandleAsync);
        Name("DeleteOrder");
        Produces(StatusCodes.Status204NoContent);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status412PreconditionFailed);
        ProducesProblem(StatusCodes.Status403Forbidden);
        ProducesProblem(StatusCodes.Status401Unauthorized);
        Summary("Delete an existing order.");
        Description("Delete an existing order");
        Policies();
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        using var _ = LogContext.PushProperty(LogProperties.OrderId, request.OrderId);
        using var __ = LogContext.PushProperty(LogProperties.CustomerId, request.CustomerId);
        
        var isAuthorize = await AuthorizationService.CanDeleteOrderAsync(
            Context, request.CustomerId).ConfigureAwait(false);
        
        if (!isAuthorize)
            return Results.Forbid();
        
        var eTag = Context.Request.Headers.IfMatch;
        var command = MapToCommand(request, eTag);
        
        var result = await SendAsync(command).ConfigureAwait(false);

        return result switch
        {
            Result.Succeed => Results.NoContent(),
            Result.InvalidEtag => Results.StatusCode((int)HttpStatusCode.PreconditionFailed),
            Result.OrderNotFound => Results.NotFound(request.OrderId),
            _ => Results.InternalServerError()
        };
    }

    private static DeleteOrderCommand MapToCommand(Request request, string? etag)
    {
        return new DeleteOrderCommand(request.CustomerId, request.OrderId, etag);
    }
}