using Order.Command.Application.Orders.Queries.GetOrders;

namespace Order.Command.API.Endpoints.GetOrders;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders", HandleAsync);
        Name("GetOrders");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        Summary("Gets the list of orders.");
        Description("Gets the list of orders.");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = request.Adapt<GetOrdersQuery>();
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = result.Adapt<Response>();
        return TypedResults.Ok(response);
    }
}