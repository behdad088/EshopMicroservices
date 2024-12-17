using Order.Command.Application.Orders.Queries.GetOrderById;

namespace Order.Command.API.Endpoints.GetOrderById;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders/{id}", HandleAsync);
        Name("GetOrdersById");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        ProducesProblem(StatusCodes.Status404NotFound);
        Summary("Gets orders by Id.");
        Description("Gets orders by Id");
    }
    
    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = MapToQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        Context.Response.Headers.ETag = $"W/\"{result.Order.Version}\"";

        var response = MapToResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrdersByIdQuery MapToQuery(Request request)
    {
        return new GetOrdersByIdQuery(request.Id);
    }

    private static Response MapToResponse(GetOrdersByIdResult result)
    {
        return new Response(result.Order);
    }
}