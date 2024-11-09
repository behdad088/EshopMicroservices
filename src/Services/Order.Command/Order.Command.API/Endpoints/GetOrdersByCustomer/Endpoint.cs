using Order.Command.Application.Orders.Queries.GetOrderByCustomer;

namespace Order.Command.API.Endpoints.GetOrdersByCustomer;

public class Endpoint : EndpointBase<Request, Response>
{
    public override void MapEndpoint()
    {
        Get("/orders/customer", HandleAsync);
        Name("GetOrdersByCustomer");
        Produces();
        ProducesProblem(StatusCodes.Status400BadRequest);
        Summary("Gets orders by customer.");
        Description("Gets orders by customer.");
    }

    public override async Task<IResult> HandleAsync(Request request)
    {
        var query = ToQuery(request);
        var result = await SendAsync(query).ConfigureAwait(false);
        var response = MapToResponse(result);
        return TypedResults.Ok(response);
    }

    private static GetOrderByCustomerQuery ToQuery(Request request)
    {
        return new GetOrderByCustomerQuery(request.CustomerId);
    }

    private static Response MapToResponse(GetOrderByCustomerResult result)
    {
        return new Response(result.Orders);
    }
}