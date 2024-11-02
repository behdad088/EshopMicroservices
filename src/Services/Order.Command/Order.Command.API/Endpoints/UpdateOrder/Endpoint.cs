using Order.Command.Application.Orders.Commands.UpdateOrder;

namespace Order.Command.API.Endpoints.UpdateOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Put("/orders", HandleAsync);
        Name("UpdateOrder");
        Produces(StatusCodes.Status204NoContent);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Update an existing order.");
        Description("Update an existing order");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var command = request.Adapt<UpdateOrderCommand>();
        var result = await SendAsync(command).ConfigureAwait(false);
        var response = result.Adapt<Response>();
        return TypedResults.Ok(response);
    }
}