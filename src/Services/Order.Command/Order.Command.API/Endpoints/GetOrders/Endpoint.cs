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
        var query = MapQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = MapResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrdersQuery MapQuery(Request request)
    {
        return new GetOrdersQuery(request.PageSize, request.PageIndex);
    }

    public static Response MapResponse(GetOrdersResult result)
    {
        return new Response(result.Orders);
    }
}