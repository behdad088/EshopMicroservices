using Order.Command.Application.Orders.Commands.DeleteOrder;

namespace Order.Command.API.Endpoints.DeleteOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Delete("/orders/{order_id}", HandleAsync);
        Name("DeleteOrder");
        Produces(StatusCodes.Status202Accepted);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Delete an existing order.");
        Description("Delete an existing order");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var command = MapToCommand(request);
        var result = await SendAsync(command).ConfigureAwait(false);
        var response = MapToResponse(result);
        return TypedResults.Ok(response);
    }

    private static DeleteOrderCommand MapToCommand(Request request)
    {
        return new DeleteOrderCommand(request.OrderId);
    }

    private static Response MapToResponse(DeleteOrderResult result)
    {
        return new Response(result.IsSuccess);
    }
}