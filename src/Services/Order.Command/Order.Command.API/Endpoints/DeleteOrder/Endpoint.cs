using Order.Command.Application.Orders.Commands.DeleteOrder;
using Order.Command.Domain.Models.ValueObjects;

namespace Order.Command.API.Endpoints.DeleteOrder;

public class Endpoint : EndpointBase<Request>
{
    public override void MapEndpoint()
    {
        Delete("/orders/{order_id}", HandleAsync);
        Name("DeleteOrder");
        Produces(StatusCodes.Status204NoContent);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        ProducesProblem(StatusCodes.Status412PreconditionFailed);
        Summary("Delete an existing order.");
        Description("Delete an existing order");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var eTag = Context.Request.Headers.IfMatch;

        var command = MapToCommand(request, eTag);
        await SendAsync(command).ConfigureAwait(false);
        return TypedResults.NoContent();
    }

    private static DeleteOrderCommand MapToCommand(Request request, string? etag)
    {
        return new DeleteOrderCommand(request.OrderId, etag);
    }
}