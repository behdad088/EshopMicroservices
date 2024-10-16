using Mapster;
using MediatR;
using Order.Command.Application.Orders.Commands.UpdateOrder;

namespace Order.Command.API.Endpoints.UpdateOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Put("/order", HandleAsync);
        Name("CreateOrder");
        Produces(StatusCodes.Status201Created);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Creates a new order");
        Description("Creates a new order");
    }

    public override async Task<IResult> HandleAsync(Request request, ISender sender)
    {
        var command = request.Adapt<UpdateOrderCommand>();
        var result = await sender.Send(command, CancellationToken).ConfigureAwait(false);
        var response = result.Adapt<Response>();
        return TypedResults.Ok(response);
    }
}