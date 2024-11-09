using Order.Command.Application.Orders.Queries.GetOrdersByName;

namespace Order.Command.API.Endpoints.GetOrdersByName;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders/name", HandleAsync);
        Name("GetOrdersByName");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Gets orders by name.");
        Description("Gets orders by name");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = MapToQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = MapToResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrdersByNameQuery MapToQuery(Request request)
    {
        return new GetOrdersByNameQuery(request.Name);
    }

    private static Response MapToResponse(GetOrdersByNameResult result)
    {
        return new Response(result.Orders);
    }
}