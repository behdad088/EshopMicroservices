using Order.Command.Application.Orders.Commands.CreateOrder;

namespace Order.Command.API.Endpoints.CreateOrder;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Post("/orders", HandleAsync);
        Name("CreateOrder");
        Produces(StatusCodes.Status201Created);
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Creates a new order");
        Description("Creates a new order");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var command = request.Adapt<CreateOrderCommand>();
        var result = await SendAsync(command).ConfigureAwait(false);
        var response = result.Adapt<Response>();
        return Results.Created($"/orders/{response.Id}", response);
    }
}