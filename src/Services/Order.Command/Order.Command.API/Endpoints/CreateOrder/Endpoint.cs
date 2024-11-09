using Order.Command.Application.Dtos;
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
        var command = ToCommand(request);
        if (command is null)
            return Results.BadRequest("Null request");

        var result = await SendAsync(command).ConfigureAwait(false);
        var response = MapResult(result);
        return Results.Created($"/orders/{response.Id}", response);
    }

    private static CreateOrderCommand? ToCommand(Request? request)
    {
        return request is null ? null : new CreateOrderCommand(request.Order);
    }

    private static Response MapResult(CreateOrderResult result)
    {
        return new Response(result.Id);
    }
}